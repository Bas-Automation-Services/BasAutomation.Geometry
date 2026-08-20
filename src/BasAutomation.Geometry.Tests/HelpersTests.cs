namespace BasAutomation.Geometry.Tests;

public class HelpersTests
{
    [Fact]
    public void Clamp_ValueInRange_ReturnsValue()
    {
        Assert.Equal(5.0, Helpers.Clamp(5.0, 0.0, 10.0));
    }

    [Fact]
    public void Clamp_ValueBelowMin_ReturnsMin()
    {
        Assert.Equal(0.0, Helpers.Clamp(-5.0, 0.0, 10.0));
    }

    [Fact]
    public void Clamp_ValueAboveMax_ReturnsMax()
    {
        Assert.Equal(10.0, Helpers.Clamp(15.0, 0.0, 10.0));
    }

    [Fact]
    public void Lerp_CalculatesCorrectly()
    {
        Assert.Equal(5.0, Helpers.Lerp(0.0, 10.0, 0.5));
        Assert.Equal(0.0, Helpers.Lerp(0.0, 10.0, 0.0));
        Assert.Equal(10.0, Helpers.Lerp(0.0, 10.0, 1.0));
    }

    [Fact]
    public void Lerp_ClampsT()
    {
        Assert.Equal(0.0, Helpers.Lerp(0.0, 10.0, -0.5));
        Assert.Equal(10.0, Helpers.Lerp(0.0, 10.0, 1.5));
    }
}