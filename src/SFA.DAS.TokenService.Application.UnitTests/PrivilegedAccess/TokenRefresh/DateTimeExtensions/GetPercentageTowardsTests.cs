using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;

namespace SFA.DAS.TokenService.Application.UnitTests.PrivilegedAccess.TokenRefresh.DateTimeExtensions
{
    [TestFixture()]
    public class GetPercentageTowardsTests
    {
        public void WhenFromIsLaterThanTo_ThenShouldReturnTimeZero()
        {
            var fromDate = DateTime.UtcNow;
            var toDate = fromDate.AddSeconds(-10);

            Assert.AreEqual(TimeSpan.Zero, fromDate.GetPercentageTowards(toDate, 50));
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

            Assert.AreEqual(expectedTotalSeconds, timespan.TotalSeconds);
        }
    }
}
