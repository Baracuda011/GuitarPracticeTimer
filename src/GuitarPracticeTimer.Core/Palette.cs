namespace GuitarPracticeTimer.Core;

/// <summary>A colour, framework-free. Each UI converts this to its own colour type.</summary>
public readonly record struct Rgb(byte R, byte G, byte B)
{
    /// <summary>As #RRGGBB, which every XAML dialect and CSS understands.</summary>
    public string ToHex() => $"#{R:X2}{G:X2}{B:X2}";
}

/// <summary>
/// The two palettes the window cross-fades between. The shift from cool to warm
/// is the app's primary signal that practice has ended - on a machine with no
/// working speaker it is the only signal.
/// </summary>
public static class Palette
{
    // Practice (calm, cool)
    public static readonly Rgb PracticeBackground = new(0x11, 0x14, 0x1A);
    public static readonly Rgb PracticeAccent = new(0x7F, 0xD1, 0xAE);

    // Cooldown (warm, unmistakably different)
    public static readonly Rgb CooldownBackground = new(0x2B, 0x1C, 0x14);
    public static readonly Rgb CooldownAccent = new(0xE8, 0xA3, 0x3D);

    /// <summary>How long the whole window takes to cross-fade between palettes.</summary>
    public static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(550);

    public static Rgb Background(bool cooldown) => cooldown ? CooldownBackground : PracticeBackground;
    public static Rgb Accent(bool cooldown) => cooldown ? CooldownAccent : PracticeAccent;
}
