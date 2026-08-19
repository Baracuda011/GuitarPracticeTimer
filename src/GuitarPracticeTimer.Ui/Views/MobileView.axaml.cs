using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace GuitarPracticeTimer.Ui.Views;

/// <summary>
/// Full-screen shell around the shared <see cref="TimerView"/> for touch
/// platforms. There is no chrome to add: no title bar to drag, no minimise or
/// close button, and no keyboard shortcuts to advertise.
/// </summary>
public partial class MobileView : UserControl
{
    /// <summary>Keeps the timer from stretching across a tablet in portrait.</summary>
    private const double PortraitWidth = 460;

    /// <summary>Buys back some of the slack a centred layout gives up, so the
    /// dial and the buttons breathe instead of sitting on top of each other.</summary>
    private const double PortraitHeight = 560;

    /// <summary>Wide enough for the dial and the controls to sit side by side.</summary>
    private const double LandscapeWidth = 900;

    /// <summary>Null until the first size arrives; the XAML defaults are portrait.</summary>
    private bool? _landscape;

    public MobileView()
    {
        InitializeComponent();
        SizeChanged += (_, e) => Adapt(e.NewSize);
    }

    /// <summary>
    /// Only this control can tell which way the device is held. TimerView sits
    /// inside a centred container and measures to its own content, so its own
    /// aspect ratio is no guide at all - it looks portrait-shaped on a rotated
    /// phone. Hence the arrangement is pushed down rather than detected there.
    /// </summary>
    private void Adapt(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) return;

        var landscape = size.Width > size.Height;
        if (_landscape == landscape) return;
        _landscape = landscape;

        // Posted rather than applied inline. These properties invalidate measure,
        // and SizeChanged fires from inside the layout pass: changing them there
        // left the first pass after a cold start with nothing drawn at all, and
        // it stayed blank until something external - a rotation - forced another
        // pass. Deferring to the next dispatcher cycle applies them to a settled
        // tree instead.
        Dispatcher.UIThread.Post(() =>
        {
            Timer.SideBySide = landscape;
            Timer.MaxWidth = landscape ? LandscapeWidth : PortraitWidth;
            Timer.MinHeight = landscape ? 0 : PortraitHeight;
        }, DispatcherPriority.Loaded);
    }
}
