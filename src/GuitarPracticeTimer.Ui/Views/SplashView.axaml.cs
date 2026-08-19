using Avalonia.Controls;

namespace GuitarPracticeTimer.Ui.Views;

/// <summary>
/// The app's own splash, shown over the timer for a moment at launch and then
/// faded out. Shared, so an iOS head can reuse it.
/// </summary>
public partial class SplashView : UserControl
{
    public SplashView() => InitializeComponent();
}
