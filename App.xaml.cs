using System.Windows;
using System.Windows.Media;

namespace GuitarPracticeTimer;

public partial class App : Application
{
    // Practice palette (calm, cool)
    public static readonly Color PracticeBg = Color.FromRgb(0x11, 0x14, 0x1A);
    public static readonly Color PracticeAccent = Color.FromRgb(0x7F, 0xD1, 0xAE);

    // Cooldown palette (warm, unmistakably different)
    public static readonly Color CooldownBg = Color.FromRgb(0x2B, 0x1C, 0x14);
    public static readonly Color CooldownAccent = Color.FromRgb(0xE8, 0xA3, 0x3D);

    protected override void OnStartup(StartupEventArgs e)
    {
        // Created in code (not XAML) so they stay unfrozen and can be colour-animated
        // on every surface that references them via DynamicResource.
        Resources["BgBrush"] = new SolidColorBrush(PracticeBg);
        Resources["AccentBrush"] = new SolidColorBrush(PracticeAccent);

        base.OnStartup(e);
    }
}
