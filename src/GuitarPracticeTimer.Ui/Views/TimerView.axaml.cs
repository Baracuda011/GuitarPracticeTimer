using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace GuitarPracticeTimer.Ui.Views;

/// <summary>
/// The dial, the buttons and the length slider - everything that is the same on
/// a desktop window and a phone screen. Platform heads supply their own chrome
/// around this and set the <see cref="TimerViewModel"/> as DataContext.
/// </summary>
public partial class TimerView : UserControl
{
    private bool _sideBySide;

    public TimerView() => InitializeComponent();

    /// <summary>
    /// Puts the dial beside the controls instead of above them. Set by the host,
    /// not detected here: this control sits inside a centred container and so
    /// measures to its own content rather than to the screen, which means its
    /// own aspect ratio says nothing about which way the device is held. The
    /// desktop window never sets it.
    /// </summary>
    public bool SideBySide
    {
        get => _sideBySide;
        set
        {
            if (_sideBySide == value) return;
            _sideBySide = value;
            ApplyArrangement();
        }
    }

    private TimerViewModel? Model => DataContext as TimerViewModel;

    private void ApplyArrangement()
    {
        if (_sideBySide)
        {
            Layout.RowDefinitions = new RowDefinitions("*");
            Layout.ColumnDefinitions = new ColumnDefinitions("Auto,*");

            Grid.SetRow(SideGroup, 0);
            Grid.SetColumn(SideGroup, 1);
            SideGroup.VerticalAlignment = VerticalAlignment.Center;
            SideGroup.Margin = new Thickness(32, 0, 0, 0);
            SideGroup.Width = 300;
        }
        else
        {
            Layout.RowDefinitions = new RowDefinitions("*,Auto");
            Layout.ColumnDefinitions = new ColumnDefinitions("*");

            Grid.SetRow(SideGroup, 1);
            Grid.SetColumn(SideGroup, 0);
            SideGroup.VerticalAlignment = VerticalAlignment.Stretch;
            SideGroup.Margin = new Thickness(0);
            SideGroup.Width = double.NaN;
        }

        Grid.SetRow(DialGroup, 0);
        Grid.SetColumn(DialGroup, 0);
    }

    private void Primary_Click(object? sender, RoutedEventArgs e) => Model?.TriggerPrimary();

    private void Reset_Click(object? sender, RoutedEventArgs e) => Model?.TriggerReset();
}
