using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GuitarPracticeTimer;

public partial class MainWindow : Window
{
    private enum Mode { Practice, Cooldown }

    /// <summary>Cooldown is fixed at three minutes.</summary>
    private const double CooldownTotal = 180;

    /// <summary>Cooldown cannot be skipped before this many seconds have passed.</summary>
    private const double CooldownLock = 60;

    private const double DialCenter = 114;
    private const double DialRadius = 100;

    private readonly DispatcherTimer _ticker;

    private Mode _mode = Mode.Practice;
    private bool _running;
    private double _total = 180;      // length of the segment currently on the dial
    private double _remaining = 180;
    private DateTime _deadlineUtc;
    private bool _ready;              // guards handlers that fire during InitializeComponent

    public MainWindow()
    {
        InitializeComponent();

        _ticker = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(40)
        };
        _ticker.Tick += OnTick;
        _ticker.Start();

        _ready = true;
        _total = DurationSlider.Value;
        _remaining = _total;
        UpdateUi();
    }

    // ---------------------------------------------------------------- ticking

    private void OnTick(object? sender, EventArgs e)
    {
        if (_running)
        {
            _remaining = (_deadlineUtc - DateTime.UtcNow).TotalSeconds;
            if (_remaining <= 0)
            {
                _remaining = 0;
                OnSegmentElapsed();
            }
        }

        UpdateUi();
    }

    private void OnSegmentElapsed()
    {
        if (_mode == Mode.Practice)
        {
            Beep();
            EnterCooldown();
        }
        else
        {
            EnterPractice();
        }
    }

    // ------------------------------------------------------------ transitions

    private void EnterCooldown()
    {
        _mode = Mode.Cooldown;
        _total = CooldownTotal;
        _remaining = CooldownTotal;
        _deadlineUtc = DateTime.UtcNow.AddSeconds(CooldownTotal);
        _running = true;
        ApplyTheme(App.CooldownBg, App.CooldownAccent);
    }

    /// <summary>Back to the main timer, reset to the slider length and paused.</summary>
    private void EnterPractice()
    {
        _mode = Mode.Practice;
        _running = false;
        _total = DurationSlider.Value;
        _remaining = _total;
        ApplyTheme(App.PracticeBg, App.PracticeAccent);
    }

    private void ApplyTheme(Color background, Color accent)
    {
        Animate("BgBrush", background);
        Animate("AccentBrush", accent);

        static void Animate(string key, Color to)
        {
            if (Application.Current.Resources[key] is not SolidColorBrush brush) return;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(550),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            });
        }
    }

    // --------------------------------------------------------------- commands

    private void Primary_Click(object sender, RoutedEventArgs e) => TriggerPrimary();

    private void TriggerPrimary()
    {
        if (_mode == Mode.Practice)
        {
            if (_running)
            {
                _remaining = Math.Max(0, (_deadlineUtc - DateTime.UtcNow).TotalSeconds);
                _running = false;
            }
            else
            {
                if (_remaining <= 0) _remaining = _total;
                _deadlineUtc = DateTime.UtcNow.AddSeconds(_remaining);
                _running = true;
            }
        }
        else if (CooldownElapsed >= CooldownLock)
        {
            EnterPractice();
        }

        UpdateUi();
    }

    private void Reset_Click(object sender, RoutedEventArgs e) => TriggerReset();

    private void TriggerReset()
    {
        if (_mode != Mode.Practice) return;   // no escape hatch out of the cooldown

        _running = false;
        _total = DurationSlider.Value;
        _remaining = _total;
        UpdateUi();
    }

    private void DurationSlider_ValueChanged(object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_ready) return;

        if (_mode == Mode.Practice && !_running)
        {
            _total = e.NewValue;
            _remaining = e.NewValue;
        }

        UpdateUi();
    }

    private double CooldownElapsed => CooldownTotal - _remaining;

    // --------------------------------------------------------------- painting

    private void UpdateUi()
    {
        var shown = (int)Math.Ceiling(_remaining - 1e-6);
        if (shown < 0) shown = 0;

        TimeText.Text = Clock(shown);
        DurationLabel.Text = Clock((int)Math.Round(DurationSlider.Value));
        ProgressArc.Data = BuildArc(_total > 0 ? _remaining / _total : 0);

        if (_mode == Mode.Practice)
        {
            ModeLabel.Text = "PRACTICE";
            SubText.Text = _running
                ? "playing"
                : Math.Abs(_remaining - _total) < 0.05 ? "ready" : "paused";

            PrimaryButton.Content = _running
                ? "Pause"
                : Math.Abs(_remaining - _total) < 0.05 ? "Start" : "Resume";
            PrimaryButton.IsEnabled = true;

            ResetButton.IsEnabled = true;
            HintText.Text = _running ? "" : "Space to start · R to reset";
            DurationSlider.IsEnabled = !_running;
        }
        else
        {
            var locked = CooldownElapsed < CooldownLock;

            ModeLabel.Text = "COOL DOWN";
            SubText.Text = "rest your hands";

            PrimaryButton.Content = "Skip cooldown";
            PrimaryButton.IsEnabled = !locked;

            ResetButton.IsEnabled = false;
            DurationSlider.IsEnabled = false;

            HintText.Text = locked
                ? $"unlocks in {Math.Max(1, (int)Math.Ceiling(CooldownLock - CooldownElapsed))}s"
                : "ready when you are";
        }
    }

    private static string Clock(int totalSeconds) =>
        $"{totalSeconds / 60}:{totalSeconds % 60:00}";

    /// <summary>Arc starting at 12 o'clock, sweeping clockwise for the given fraction.</summary>
    private static Geometry? BuildArc(double fraction)
    {
        if (fraction <= 0) return null;
        var sweep = Math.Min(fraction, 0.99999) * 360.0;

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(PointOnDial(0), false, false);
            ctx.ArcTo(PointOnDial(sweep), new Size(DialRadius, DialRadius),
                0, sweep > 180, SweepDirection.Clockwise, true, false);
        }
        geometry.Freeze();
        return geometry;

        static Point PointOnDial(double degreesFromTop)
        {
            var rad = (degreesFromTop - 90) * Math.PI / 180.0;
            return new Point(DialCenter + DialRadius * Math.Cos(rad),
                             DialCenter + DialRadius * Math.Sin(rad));
        }
    }

    private static void Beep() => Task.Run(() =>
    {
        try
        {
            Console.Beep(784, 150);
            Thread.Sleep(70);
            Console.Beep(1047, 280);
        }
        catch
        {
            // No speaker / beep unavailable — the colour change is the real signal.
        }
    });

    // ----------------------------------------------------------------- chrome

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Space:
                TriggerPrimary();
                e.Handled = true;
                break;
            case Key.R:
                TriggerReset();
                e.Handled = true;
                break;
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
        }

        base.OnPreviewKeyDown(e);
    }

    private void Chrome_Drag(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch (InvalidOperationException) { }
        }
    }

    private void Minimise_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
