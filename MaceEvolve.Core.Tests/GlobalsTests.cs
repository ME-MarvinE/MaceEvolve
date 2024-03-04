namespace MaceEvolve.Core.Tests
{
    public class GlobalsTests
    {
        [Theory]
        [InlineData(90, 270, -180)]
        [InlineData(270, 90, 180)]
        [InlineData(355, 5, -10)]
        [InlineData(5, 355, 10)]
        [InlineData(360, 0, 0)]
        [InlineData(0, 360, 0)]
        public void AngleDifferenceReturnsSmallestValueToRotateBy(float angle1, float angle2, float expected)
        {
            float difference = Globals.AngleDifference(angle1, angle2);
            Assert.Equal(expected, difference);
        }
    }
}