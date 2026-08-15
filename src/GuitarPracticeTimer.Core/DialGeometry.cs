namespace GuitarPracticeTimer.Core;

/// <summary>A point on the dial, in the dial's own coordinate space.</summary>
public readonly record struct DialPoint(double X, double Y);

/// <summary>
/// The arc maths, kept free of any drawing framework so the Avalonia view today
/// and any future canvas can each build their own path object from the same
/// numbers.
/// </summary>
public static class DialGeometry
{
    public const double Centre = 114;
    public const double Radius = 100;

    /// <summary>
    /// A full circle is clamped just short of 360° — a sweep of exactly 360°
    /// puts the arc's end point on top of its start point, which collapses the
    /// path to nothing and makes a completed dial vanish.
    /// </summary>
    private const double MaxFraction = 0.99999;

    /// <summary>Degrees to sweep clockwise from 12 o'clock for the given fill, 0 to 1.</summary>
    public static double SweepDegrees(double fraction) =>
        Math.Min(fraction, MaxFraction) * 360.0;

    /// <summary>Whether an arc of this sweep needs the large-arc flag set.</summary>
    public static bool IsLargeArc(double sweepDegrees) => sweepDegrees > 180;

    /// <summary>Whether there is any arc to draw at all.</summary>
    public static bool HasArc(double fraction) => fraction > 0;

    /// <summary>A point on the dial, measured clockwise from 12 o'clock.</summary>
    public static DialPoint PointOnDial(double degreesFromTop,
        double centre = Centre, double radius = Radius)
    {
        var rad = (degreesFromTop - 90) * Math.PI / 180.0;
        return new DialPoint(centre + radius * Math.Cos(rad),
                             centre + radius * Math.Sin(rad));
    }
}
