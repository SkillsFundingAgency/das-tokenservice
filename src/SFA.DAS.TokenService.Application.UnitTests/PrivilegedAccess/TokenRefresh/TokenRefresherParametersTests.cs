using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;

namespace SFA.DAS.TokenService.Application.UnitTests.PrivilegedAccess.TokenRefresh
{
    [TestFixture()]
    public class TokenRefresherParametersTests
    {
        [TestCase(50, 80)]
        [TestCase(80, 80)]
        [TestCase(90, 90)]
        [TestCase(100, 100)]
        [TestCase(101, 100)]
        public void Test(int setValue, int expectedValue)
        {
            var parameters = new TokenRefresherParameters();
            parameters.TokenRefreshExpirationPercentage = setValue;

            Assert.AreEqual(expectedValue, parameters.TokenRefreshExpirationPercentage);
        }
    }
}
