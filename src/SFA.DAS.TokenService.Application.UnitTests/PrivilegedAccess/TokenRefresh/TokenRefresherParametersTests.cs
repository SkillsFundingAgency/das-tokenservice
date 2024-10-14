using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;

namespace SFA.DAS.TokenService.Application.UnitTests.PrivilegedAccess.TokenRefresh;

public class TokenRefresherParametersTests
{
    [TestCase(50, 80)]
    [TestCase(80, 80)]
    [TestCase(90, 90)]
    [TestCase(100, 100)]
    [TestCase(101, 100)]
    public void Test(int setValue, int expectedValue)
    {
        var parameters = new TokenRefresherParameters
        {
            TokenRefreshExpirationPercentage = setValue
        };

        expectedValue.Should().Be(parameters.TokenRefreshExpirationPercentage);
    }
}