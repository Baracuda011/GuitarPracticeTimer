using Avalonia.Controls;

namespace GuitarPracticeTimer.Ui.Views;

/// <summary>
/// Full-screen shell around the shared <see cref="TimerView"/> for touch
/// platforms. There is no chrome to add: no title bar to drag, no minimise or
/// close button, and no keyboard shortcuts to advertise.
/// </summary>
public partial class MobileView : UserControl
{
    public MobileView() => InitializeComponent();
}
