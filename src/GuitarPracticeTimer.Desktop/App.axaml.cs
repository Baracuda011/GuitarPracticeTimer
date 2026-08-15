using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GuitarPracticeTimer.Ui;
using GuitarPracticeTimer.Ui.Audio;

namespace GuitarPracticeTimer.Desktop;

public partial class App : Application
{
    private TimerViewModel? _model;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _model = new TimerViewModel(new Chime(), keyboardHints: true);
            desktop.MainWindow = new MainWindow { DataContext = _model };
            desktop.ShutdownRequested += (_, _) => _model.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
