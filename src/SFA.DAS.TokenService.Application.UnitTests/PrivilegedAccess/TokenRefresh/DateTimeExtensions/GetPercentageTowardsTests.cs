using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;

namespace SFA.DAS.TokenService.Application.UnitTests.PrivilegedAccess.TokenRefresh.DateTimeExtensions;

public class GetPercentageTowardsTests
{
    [Test]
    public void WhenFromIsLaterThanTo_ThenShouldReturnTimeZero()
    {
        var fromDate = DateTime.UtcNow;
        var toDate = fromDate.AddSeconds(-10);

        
        fromDate.GetPercentageTowards(toDate, 50).Should().Be(TimeSpan.Zero);
    }

    [TestCase(50, 80, 40)]
    [TestCase(100, 80, 80)]
    [TestCase(150, 0, 0)]
    [TestCase(150, 100, 150)]
    public void WhenSpecifiedSecondsBetweenFromAndTo_ThenPercentageShouldBeExpectedNumberOfSeconds(int secondsBetweenFromAndTo, int gotoPercentage, int expectedTotalSeconds)
    {
        var fromDate = DateTime.UtcNow;
        var toDate = fromDate.AddSeconds(secondsBetweenFromAndTo);

        var timespan = fromDate.GetPercentageTowards(toDate, gotoPercentage);

        expectedTotalSeconds.Should().Be((int)timespan.TotalSeconds);
    }
}