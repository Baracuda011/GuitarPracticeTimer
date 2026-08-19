namespace GuitarPracticeTimer.Core;

/// <summary>Which half of the cycle the session is in.</summary>
public enum SessionMode { Practice, Cooldown }

/// <summary>What a call to the session changed, if anything.</summary>
public enum SessionEvent { None, EnteredCooldown, EnteredPractice }

/// <summary>
/// The whole timer, with no UI framework attached. Every rule that makes the
/// cooldown hard to escape lives here and nowhere else, so desktop and mobile
/// cannot drift apart on the one thing the app exists to enforce.
/// </summary>
/// <remarks>
/// The caller supplies the current UTC time on every method that needs one.
/// That keeps the class free of a hidden clock, makes the enforcement rules
/// testable without waiting three real minutes, and - the reason it matters on
/// mobile - means a session resumed after the OS suspended the app recomputes
/// from its deadline instead of from ticks it never received.
/// </remarks>
public sealed class PracticeSession
{
    /// <summary>Cooldown is fixed at three minutes.</summary>
    public const double CooldownTotalSeconds = 180;

    /// <summary>Cooldown cannot be skipped before this many seconds have passed.</summary>
    public const double CooldownLockSeconds = 60;

    public const double MinDurationSeconds = 30;
    public const double MaxDurationSeconds = 600;
    public const double DurationStepSeconds = 10;
    public const double DefaultDurationSeconds = 180;

    /// <summary>Practice and cooldown are "the same" length within this tolerance.</summary>
    private const double Epsilon = 0.05;

    private DateTime _deadlineUtc;

    public SessionMode Mode { get; private set; } = SessionMode.Practice;

    public bool IsRunning { get; private set; }

    /// <summary>The practice length currently selected, i.e. where the slider sits.</summary>
    public double DurationSeconds { get; private set; } = DefaultDurationSeconds;

    /// <summary>Length of the segment currently on the dial - practice length or cooldown.</summary>
    public double TotalSeconds { get; private set; } = DefaultDurationSeconds;

    public double RemainingSeconds { get; private set; } = DefaultDurationSeconds;

    public double CooldownElapsedSeconds => CooldownTotalSeconds - RemainingSeconds;

    /// <summary>True while the cooldown refuses to be skipped.</summary>
    public bool IsCooldownLocked =>
        Mode == SessionMode.Cooldown && CooldownElapsedSeconds < CooldownLockSeconds;

    /// <summary>How much of the dial is still filled, 0 to 1.</summary>
    public double RemainingFraction => TotalSeconds > 0 ? RemainingSeconds / TotalSeconds : 0;

    /// <summary>The segment is untouched - neither started nor part-way through.</summary>
    private bool AtFullSegment => Math.Abs(RemainingSeconds - TotalSeconds) < Epsilon;

    // ---------------------------------------------------------------- ticking

    /// <summary>Advance to <paramref name="utcNow"/>, rolling over if the segment ran out.</summary>
    public SessionEvent Tick(DateTime utcNow)
    {
        if (!IsRunning) return SessionEvent.None;

        RemainingSeconds = (_deadlineUtc - utcNow).TotalSeconds;
        if (RemainingSeconds > 0) return SessionEvent.None;

        RemainingSeconds = 0;
        return Mode == SessionMode.Practice
            ? EnterCooldown(utcNow)
            : EnterPractice();
    }

    // --------------------------------------------------------------- commands

    /// <summary>
    /// Start, pause or resume practice; during cooldown this is the skip button,
    /// and it does nothing at all until the lock has expired.
    /// </summary>
    public SessionEvent TogglePrimary(DateTime utcNow)
    {
        if (Mode == SessionMode.Cooldown)
        {
            return CooldownElapsedSeconds >= CooldownLockSeconds
                ? EnterPractice()
                : SessionEvent.None;
        }

        if (IsRunning)
        {
            RemainingSeconds = Math.Max(0, (_deadlineUtc - utcNow).TotalSeconds);
            IsRunning = false;
        }
        else
        {
            if (RemainingSeconds <= 0) RemainingSeconds = TotalSeconds;
            _deadlineUtc = utcNow.AddSeconds(RemainingSeconds);
            IsRunning = true;
        }

        return SessionEvent.None;
    }

    /// <summary>Reset is a no-op for the whole break - there is no escape hatch.</summary>
    /// <returns>Whether the reset was allowed.</returns>
    public bool Reset()
    {
        if (Mode != SessionMode.Practice) return false;

        IsRunning = false;
        TotalSeconds = DurationSeconds;
        RemainingSeconds = DurationSeconds;
        return true;
    }

    /// <summary>Move the practice-length selection. Ignored unless practice is idle.</summary>
    /// <returns>Whether the change was allowed.</returns>
    public bool SetDuration(double seconds)
    {
        DurationSeconds = Math.Clamp(seconds, MinDurationSeconds, MaxDurationSeconds);

        if (Mode != SessionMode.Practice || IsRunning) return false;

        TotalSeconds = DurationSeconds;
        RemainingSeconds = DurationSeconds;
        return true;
    }

    // ------------------------------------------------------------ transitions

    private SessionEvent EnterCooldown(DateTime utcNow)
    {
        Mode = SessionMode.Cooldown;
        TotalSeconds = CooldownTotalSeconds;
        RemainingSeconds = CooldownTotalSeconds;
        _deadlineUtc = utcNow.AddSeconds(CooldownTotalSeconds);
        IsRunning = true;
        return SessionEvent.EnteredCooldown;
    }

    /// <summary>Back to the main timer, reset to the selected length and paused.</summary>
    private SessionEvent EnterPractice()
    {
        Mode = SessionMode.Practice;
        IsRunning = false;
        TotalSeconds = DurationSeconds;
        RemainingSeconds = DurationSeconds;
        return SessionEvent.EnteredPractice;
    }

    // --------------------------------------------------------------- describing

    /// <summary>
    /// Everything the UI needs to paint one frame. <paramref name="keyboardHints"/>
    /// is false on touch platforms, where "Space to start" would be a lie.
    /// </summary>
    public SessionView Describe(bool keyboardHints = true)
    {
        if (Mode == SessionMode.Practice)
        {
            return new SessionView(
                Time: Clock(RemainingSeconds),
                Duration: Clock(DurationSeconds),
                Mode: "PRACTICE",
                Status: IsRunning ? "playing" : AtFullSegment ? "ready" : "paused",
                PrimaryLabel: IsRunning ? "Pause" : AtFullSegment ? "Start" : "Resume",
                Hint: IsRunning || !keyboardHints ? "" : "Space to start · R to reset",
                PrimaryEnabled: true,
                ResetEnabled: true,
                DurationEnabled: !IsRunning,
                RemainingFraction: RemainingFraction,
                IsCooldown: false);
        }

        var locked = IsCooldownLocked;
        var unlocksIn = Math.Max(1, (int)Math.Ceiling(CooldownLockSeconds - CooldownElapsedSeconds));

        return new SessionView(
            Time: Clock(RemainingSeconds),
            Duration: Clock(DurationSeconds),
            Mode: "COOL DOWN",
            Status: "rest your hands",
            PrimaryLabel: "Skip cooldown",
            Hint: locked ? $"unlocks in {unlocksIn}s" : "ready when you are",
            PrimaryEnabled: !locked,
            ResetEnabled: false,
            DurationEnabled: false,
            RemainingFraction: RemainingFraction,
            IsCooldown: true);
    }

    /// <summary>Seconds as m:ss, rounded up so the clock reads 3:00 the instant it starts.</summary>
    public static string Clock(double seconds)
    {
        var shown = (int)Math.Ceiling(seconds - 1e-6);
        if (shown < 0) shown = 0;
        return $"{shown / 60}:{shown % 60:00}";
    }
}
