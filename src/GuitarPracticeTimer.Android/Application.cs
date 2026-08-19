using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace GuitarPracticeTimer.Droid;

[Application(Label = "Practice Timer", Icon = "@mipmap/appicon")]
public class MainApplication : AvaloniaAndroidApplication<App>
{
    protected MainApplication(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).WithInterFont();
}
