using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
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
    /// <summary>How long the splash holds before it starts to go.</summary>
    private static readonly TimeSpan SplashHold = TimeSpan.FromMilliseconds(1100);

    /// <summary>Matches the app's own colour cross-fade, so the launch feels
    /// like the same piece of software throughout.</summary>
    private static readonly TimeSpan SplashFade = TimeSpan.FromMilliseconds(550);

    private TimerViewModel? _model;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            // keyboardHints: false - "Space to start" is meaningless on a phone,
            // and the view model already suppresses that line when asked.
            _model = new TimerViewModel(new Chime(), keyboardHints: false);

            var timer = new MobileView { DataContext = _model };
            var splash = new SplashView
            {
                Transitions =
                [
                    new DoubleTransition
                    {
                        Property = Visual.OpacityProperty,
                        Duration = SplashFade
                    }
                ]
            };

            var root = new Panel();
            root.Children.Add(timer);
            root.Children.Add(splash);
            singleView.MainView = root;

            DispatcherTimer.RunOnce(() =>
            {
                splash.IsHitTestVisible = false;
                splash.Opacity = 0;

                // Taken out of the tree once it is invisible; leaving it there
                // would swallow nothing but still cost a layout pass per frame.
                DispatcherTimer.RunOnce(() => root.Children.Remove(splash), SplashFade);
            }, SplashHold);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
