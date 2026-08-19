using Avalonia;
using Avalonia.Controls;

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
        var landscape = size.Width > size.Height;

        Timer.SideBySide = landscape;
        Timer.MaxWidth = landscape ? LandscapeWidth : PortraitWidth;
        Timer.MinHeight = landscape ? 0 : PortraitHeight;
    }
}
