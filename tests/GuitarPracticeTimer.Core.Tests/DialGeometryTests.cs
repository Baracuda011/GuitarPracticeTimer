using GuitarPracticeTimer.Core;
using Xunit;

namespace GuitarPracticeTimer.Core.Tests;

public class DialGeometryTests
{
    [Fact]
    public void An_empty_dial_has_no_arc() => Assert.False(DialGeometry.HasArc(0));

    [Fact]
    public void A_full_dial_stops_just_short_of_a_whole_turn()
    {
        // A sweep of exactly 360 would collapse the path to a zero-length line.
        var sweep = DialGeometry.SweepDegrees(1.0);

        Assert.True(sweep < 360);
        Assert.True(sweep > 359.99);
    }

    [Theory]
    [InlineData(0.25, 90)]
    [InlineData(0.5, 180)]
    [InlineData(0.75, 270)]
    public void Sweep_is_proportional_to_the_fill(double fraction, double expected) =>
        Assert.Equal(expected, DialGeometry.SweepDegrees(fraction), 3);

    [Theory]
    [InlineData(0.25, false)]
    [InlineData(0.5, false)]
    [InlineData(0.51, true)]
    public void The_large_arc_flag_is_set_past_a_half_turn(double fraction, bool expected) =>
        Assert.Equal(expected, DialGeometry.IsLargeArc(DialGeometry.SweepDegrees(fraction)));

    [Fact]
    public void The_arc_starts_at_twelve_o_clock()
    {
        var start = DialGeometry.PointOnDial(0);

        Assert.Equal(DialGeometry.Centre, start.X, 6);
        Assert.Equal(DialGeometry.Centre - DialGeometry.Radius, start.Y, 6);
    }

    [Fact]
    public void The_dial_sweeps_clockwise()
    {
        // A quarter turn from the top must land on the right-hand side, not the left.
        var quarter = DialGeometry.PointOnDial(90);

        Assert.Equal(DialGeometry.Centre + DialGeometry.Radius, quarter.X, 6);
        Assert.Equal(DialGeometry.Centre, quarter.Y, 6);
    }
}
