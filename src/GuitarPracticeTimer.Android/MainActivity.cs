using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace GuitarPracticeTimer.Droid;

[Activity(
    Label = "Practice Timer",
    Theme = "@style/AppTheme.NoActionBar",
    Icon = "@mipmap/appicon",
    MainLauncher = true,
    // Handling these ourselves stops Android recreating the activity on rotation
    // or a theme change. The timer is deadline-based so it would survive a
    // recreate, but rebuilding the view mid-block is a visible stutter.
    ConfigurationChanges = ConfigChanges.Orientation
                         | ConfigChanges.ScreenSize
                         | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
