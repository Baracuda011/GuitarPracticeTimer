using GuitarPracticeTimer.Core;
using Xunit;

namespace GuitarPracticeTimer.Core.Tests;

public class PracticeSessionTests
{
    private static readonly DateTime T0 = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static PracticeSession Started()
    {
        var session = new PracticeSession();
        session.TogglePrimary(T0);
        return session;
    }

    /// <summary>Runs practice to exhaustion and returns the session sitting in cooldown.</summary>
    private static PracticeSession InCooldown()
    {
        var session = Started();
        session.Tick(T0.AddSeconds(PracticeSession.DefaultDurationSeconds));
        return session;
    }

    // ------------------------------------------------------------------ basics

    [Fact]
    public void Starts_idle_in_practice_at_the_default_length()
    {
        var session = new PracticeSession();
        var view = session.Describe();

        Assert.Equal(SessionMode.Practice, session.Mode);
        Assert.False(session.IsRunning);
        Assert.Equal("3:00", view.Time);
        Assert.Equal("ready", view.Status);
        Assert.Equal("Start", view.PrimaryLabel);
    }

    [Fact]
    public void Practice_running_out_enters_cooldown()
    {
        var session = Started();

        var result = session.Tick(T0.AddSeconds(PracticeSession.DefaultDurationSeconds));

        Assert.Equal(SessionEvent.EnteredCooldown, result);
        Assert.Equal(SessionMode.Cooldown, session.Mode);
        Assert.True(session.IsRunning);
        Assert.Equal(PracticeSession.CooldownTotalSeconds, session.RemainingSeconds, 3);
    }

    [Fact]
    public void Pausing_preserves_the_remaining_time_and_resuming_continues_from_it()
    {
        var session = Started();

        session.TogglePrimary(T0.AddSeconds(50));
        Assert.False(session.IsRunning);
        Assert.Equal(130, session.RemainingSeconds, 3);

        // Ten minutes pass while paused; the clock must not have moved.
        session.Tick(T0.AddSeconds(650));
        Assert.Equal(130, session.RemainingSeconds, 3);

        session.TogglePrimary(T0.AddSeconds(650));
        session.Tick(T0.AddSeconds(660));
        Assert.Equal(120, session.RemainingSeconds, 3);
    }

    // ------------------------------------------- the cooldown must not be escapable

    [Fact]
    public void Skip_does_nothing_while_the_cooldown_lock_holds()
    {
        var session = InCooldown();
        var cooldownStart = T0.AddSeconds(PracticeSession.DefaultDurationSeconds);

        session.Tick(cooldownStart.AddSeconds(PracticeSession.CooldownLockSeconds - 1));
        Assert.True(session.IsCooldownLocked);
        Assert.False(session.Describe().PrimaryEnabled);

        var result = session.TogglePrimary(cooldownStart.AddSeconds(59));

        Assert.Equal(SessionEvent.None, result);
        Assert.Equal(SessionMode.Cooldown, session.Mode);
    }

    [Fact]
    public void Skip_is_allowed_once_the_lock_expires()
    {
        var session = InCooldown();
        var cooldownStart = T0.AddSeconds(PracticeSession.DefaultDurationSeconds);
        var unlocked = cooldownStart.AddSeconds(PracticeSession.CooldownLockSeconds);

        session.Tick(unlocked);
        Assert.False(session.IsCooldownLocked);

        var result = session.TogglePrimary(unlocked);

        Assert.Equal(SessionEvent.EnteredPractice, result);
        Assert.Equal(SessionMode.Practice, session.Mode);
        Assert.False(session.IsRunning);
    }

    [Fact]
    public void Reset_is_refused_for_the_whole_cooldown()
    {
        var session = InCooldown();
        var cooldownStart = T0.AddSeconds(PracticeSession.DefaultDurationSeconds);

        // Even well past the skip lock, reset is still not an escape hatch.
        session.Tick(cooldownStart.AddSeconds(170));

        Assert.False(session.Reset());
        Assert.Equal(SessionMode.Cooldown, session.Mode);
        Assert.False(session.Describe().ResetEnabled);
    }

    [Fact]
    public void Duration_cannot_be_changed_during_the_cooldown()
    {
        var session = InCooldown();

        Assert.False(session.SetDuration(30));

        Assert.Equal(PracticeSession.CooldownTotalSeconds, session.TotalSeconds, 3);
        Assert.False(session.Describe().DurationEnabled);
    }

    [Fact]
    public void Duration_cannot_be_changed_while_practice_is_running()
    {
        var session = Started();

        Assert.False(session.SetDuration(600));

        Assert.Equal(PracticeSession.DefaultDurationSeconds, session.TotalSeconds, 3);
        Assert.False(session.Describe().DurationEnabled);
    }

    [Fact]
    public void Cooldown_runs_its_full_length_then_returns_to_practice()
    {
        var session = InCooldown();
        var cooldownStart = T0.AddSeconds(PracticeSession.DefaultDurationSeconds);

        var result = session.Tick(cooldownStart.AddSeconds(PracticeSession.CooldownTotalSeconds));

        Assert.Equal(SessionEvent.EnteredPractice, result);
        Assert.Equal(SessionMode.Practice, session.Mode);
        Assert.False(session.IsRunning);
    }

    // ------------------------------------------------- deadline-based timekeeping

    [Fact]
    public void A_long_gap_between_ticks_still_lands_in_the_right_state()
    {
        // Models the OS suspending the app: no ticks arrive for twenty minutes.
        var session = InCooldown();
        var cooldownStart = T0.AddSeconds(PracticeSession.DefaultDurationSeconds);

        var result = session.Tick(cooldownStart.AddSeconds(1200));

        Assert.Equal(SessionEvent.EnteredPractice, result);
        Assert.Equal(SessionMode.Practice, session.Mode);
    }

    [Fact]
    public void Ticking_while_idle_changes_nothing()
    {
        var session = new PracticeSession();

        var result = session.Tick(T0.AddSeconds(9999));

        Assert.Equal(SessionEvent.None, result);
        Assert.Equal(PracticeSession.DefaultDurationSeconds, session.RemainingSeconds, 3);
    }

    // ----------------------------------------------------------------- duration

    [Theory]
    [InlineData(10, PracticeSession.MinDurationSeconds)]
    [InlineData(900, PracticeSession.MaxDurationSeconds)]
    [InlineData(240, 240)]
    public void Duration_is_clamped_to_the_supported_range(double requested, double expected)
    {
        var session = new PracticeSession();

        session.SetDuration(requested);

        Assert.Equal(expected, session.DurationSeconds, 3);
    }

    [Fact]
    public void Returning_to_practice_adopts_the_selected_duration()
    {
        var session = new PracticeSession();
        session.SetDuration(300);
        session.TogglePrimary(T0);
        session.Tick(T0.AddSeconds(300));                       // into cooldown
        session.Tick(T0.AddSeconds(300 + PracticeSession.CooldownTotalSeconds));

        Assert.Equal(SessionMode.Practice, session.Mode);
        Assert.Equal(300, session.TotalSeconds, 3);
        Assert.Equal("5:00", session.Describe().Time);
    }

    // -------------------------------------------------------------- presentation

    [Theory]
    [InlineData(180, "3:00")]
    [InlineData(0, "0:00")]
    [InlineData(61, "1:01")]
    [InlineData(59.4, "1:00")]
    [InlineData(-5, "0:00")]
    [InlineData(600, "10:00")]
    public void Clock_formats_seconds_as_m_ss(double seconds, string expected) =>
        Assert.Equal(expected, PracticeSession.Clock(seconds));

    [Fact]
    public void Keyboard_hints_are_suppressed_for_touch_platforms()
    {
        var session = new PracticeSession();

        Assert.Contains("Space", session.Describe(keyboardHints: true).Hint);
        Assert.Equal("", session.Describe(keyboardHints: false).Hint);
    }

    [Fact]
    public void The_locked_cooldown_counts_down_to_its_unlock()
    {
        var session = InCooldown();
        var cooldownStart = T0.AddSeconds(PracticeSession.DefaultDurationSeconds);

        session.Tick(cooldownStart.AddSeconds(30));

        Assert.Equal("unlocks in 30s", session.Describe().Hint);
    }
}
