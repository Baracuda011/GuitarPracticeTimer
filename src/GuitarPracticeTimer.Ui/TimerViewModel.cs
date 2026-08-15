using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using Avalonia.Threading;
using GuitarPracticeTimer.Core;
using GuitarPracticeTimer.Ui.Audio;

namespace GuitarPracticeTimer.Ui;

/// <summary>
/// Drives the UI from a <see cref="PracticeSession"/>. Shared by every platform
/// head — the desktop window and the mobile page bind to the same instance type
/// and differ only in how they lay the same values out.
/// </summary>
/// <remarks>
/// Hand-rolled <see cref="INotifyPropertyChanged"/> rather than an MVVM toolkit:
/// there are a dozen properties and no commands, so a dependency would buy
/// nothing. Colour changes are published as whole brushes and animated by
/// <c>BrushTransition</c> in XAML, which is Avalonia's equivalent of the
/// animated <c>DynamicResource</c> brush the WPF build used.
/// </remarks>
public sealed class TimerViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly PracticeSession _session = new();
    private readonly IChime _chime;
    private readonly bool _keyboardHints;
    private readonly DispatcherTimer _ticker;

    private string _time = "3:00";
    private string _durationLabel = "3:00";
    private string _mode = "PRACTICE";
    private string _status = "ready";
    private string _primaryLabel = "Start";
    private string _hint = "";
    private bool _primaryEnabled = true;
    private bool _resetEnabled = true;
    private bool _durationEnabled = true;
    private Geometry? _arc;
    private ISolidColorBrush _background = Brush(Palette.PracticeBackground);
    private ISolidColorBrush _accent = Brush(Palette.PracticeAccent);

    /// <param name="chime">Pass <see cref="SilentChime"/> where there is no audio stack.</param>
    /// <param name="keyboardHints">False on touch platforms, where "Space to start" is a lie.</param>
    public TimerViewModel(IChime? chime = null, bool keyboardHints = true)
    {
        _chime = chime ?? new SilentChime();
        _keyboardHints = keyboardHints;

        _ticker = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(40)
        };
        _ticker.Tick += OnTick;
        _ticker.Start();

        Refresh();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    // ------------------------------------------------------------- display

    public string Time { get => _time; private set => Set(ref _time, value); }
    public string DurationLabel { get => _durationLabel; private set => Set(ref _durationLabel, value); }
    public string Mode { get => _mode; private set => Set(ref _mode, value); }
    public string Status { get => _status; private set => Set(ref _status, value); }
    public string PrimaryLabel { get => _primaryLabel; private set => Set(ref _primaryLabel, value); }
    public string Hint { get => _hint; private set => Set(ref _hint, value); }

    public bool PrimaryEnabled { get => _primaryEnabled; private set => Set(ref _primaryEnabled, value); }
    public bool ResetEnabled { get => _resetEnabled; private set => Set(ref _resetEnabled, value); }
    public bool DurationEnabled { get => _durationEnabled; private set => Set(ref _durationEnabled, value); }

    public Geometry? Arc { get => _arc; private set => Set(ref _arc, value); }

    public ISolidColorBrush Background { get => _background; private set => Set(ref _background, value); }
    public ISolidColorBrush Accent { get => _accent; private set => Set(ref _accent, value); }

    // ------------------------------------------------------------- slider

    public double MinDuration => PracticeSession.MinDurationSeconds;
    public double MaxDuration => PracticeSession.MaxDurationSeconds;
    public double DurationStep => PracticeSession.DurationStepSeconds;

    public double DurationSeconds
    {
        get => _session.DurationSeconds;
        set
        {
            if (Math.Abs(_session.DurationSeconds - value) < 0.001) return;
            _session.SetDuration(value);
            OnPropertyChanged();
            Refresh();
        }
    }

    // ------------------------------------------------------------ commands

    public void TriggerPrimary() => Handle(_session.TogglePrimary(DateTime.UtcNow));

    public void TriggerReset()
    {
        _session.Reset();
        Refresh();
    }

    private void OnTick(object? sender, EventArgs e) => Handle(_session.Tick(DateTime.UtcNow));

    private void Handle(SessionEvent change)
    {
        if (change == SessionEvent.EnteredCooldown) _chime.Play();
        if (change != SessionEvent.None) ApplyPalette();
        Refresh();
    }

    // ------------------------------------------------------------ painting

    private void Refresh()
    {
        var view = _session.Describe(_keyboardHints);

        Time = view.Time;
        DurationLabel = view.Duration;
        Mode = view.Mode;
        Status = view.Status;
        PrimaryLabel = view.PrimaryLabel;
        Hint = view.Hint;
        PrimaryEnabled = view.PrimaryEnabled;
        ResetEnabled = view.ResetEnabled;
        DurationEnabled = view.DurationEnabled;
        Arc = BuildArc(view.RemainingFraction);
    }

    /// <summary>
    /// Replaces both brushes outright; the 550 ms cross-fade is declared as a
    /// BrushTransition on each control that binds to them.
    /// </summary>
    private void ApplyPalette()
    {
        var cooldown = _session.Mode == SessionMode.Cooldown;
        Background = Brush(Palette.Background(cooldown));
        Accent = Brush(Palette.Accent(cooldown));
    }

    private static ISolidColorBrush Brush(Rgb rgb) =>
        new SolidColorBrush(Color.FromRgb(rgb.R, rgb.G, rgb.B));

    /// <summary>Arc starting at 12 o'clock, sweeping clockwise for the given fraction.</summary>
    private static Geometry? BuildArc(double fraction)
    {
        if (!DialGeometry.HasArc(fraction)) return null;

        var sweep = DialGeometry.SweepDegrees(fraction);
        var start = DialGeometry.PointOnDial(0);
        var end = DialGeometry.PointOnDial(sweep);

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Avalonia.Point(start.X, start.Y), false);
            ctx.ArcTo(new Avalonia.Point(end.X, end.Y),
                new Avalonia.Size(DialGeometry.Radius, DialGeometry.Radius),
                0, DialGeometry.IsLargeArc(sweep), SweepDirection.Clockwise);
            ctx.EndFigure(false);
        }

        return geometry;
    }

    // ---------------------------------------------------------------- INPC

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(name);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public void Dispose()
    {
        _ticker.Stop();
        _ticker.Tick -= OnTick;
        (_chime as IDisposable)?.Dispose();
    }
}
