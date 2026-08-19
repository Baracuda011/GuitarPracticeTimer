using Avalonia.Controls;
using Avalonia.Interactivity;

namespace GuitarPracticeTimer.Ui.Views;

/// <summary>
/// The dial, the buttons and the length slider - everything that is the same on
/// a desktop window and a phone screen. Platform heads supply their own chrome
/// around this and set the <see cref="TimerViewModel"/> as DataContext.
/// </summary>
public partial class TimerView : UserControl
{
    public TimerView() => InitializeComponent();

    private TimerViewModel? Model => DataContext as TimerViewModel;

    private void Primary_Click(object? sender, RoutedEventArgs e) => Model?.TriggerPrimary();

    private void Reset_Click(object? sender, RoutedEventArgs e) => Model?.TriggerReset();
}
