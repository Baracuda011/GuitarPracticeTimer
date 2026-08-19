using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GuitarPracticeTimer.Ui;
using GuitarPracticeTimer.Ui.Audio;
using GuitarPracticeTimer.Ui.Views;

namespace GuitarPracticeTimer.Droid;

/// <summary>
/// Android bootstrap. Mobile uses a single-view lifetime rather than a window,
/// so the shared <see cref="MobileView"/> becomes the root control.
/// </summary>
// Fully qualified: Android's implicit usings bring in Android.App.Application,
// which collides with Avalonia's.
public partial class App : Avalonia.Application
{
    private TimerViewModel? _model;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            // keyboardHints: false - "Space to start" is meaningless on a phone,
            // and the view model already suppresses that line when asked.
            _model = new TimerViewModel(new Chime(), keyboardHints: false);
            singleView.MainView = new MobileView { DataContext = _model };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
