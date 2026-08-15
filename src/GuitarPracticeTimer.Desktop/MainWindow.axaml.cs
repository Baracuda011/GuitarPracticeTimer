using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GuitarPracticeTimer.Ui;

namespace GuitarPracticeTimer.Desktop;

/// <summary>
/// Desktop chrome around the shared <see cref="Ui.Views.TimerView"/>: a custom
/// title bar, drag-to-move, and the keyboard shortcuts. Everything here is
/// desktop-only by nature and has no equivalent on a phone.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private TimerViewModel? Model => DataContext as TimerViewModel;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Space:
                Model?.TriggerPrimary();
                e.Handled = true;
                break;
            case Key.R:
                Model?.TriggerReset();
                e.Handled = true;
                break;
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
        }

        base.OnKeyDown(e);
    }

    private void Chrome_Drag(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void Minimise_Click(object? sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();
}
