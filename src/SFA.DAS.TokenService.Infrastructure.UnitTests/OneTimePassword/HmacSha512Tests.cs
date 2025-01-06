using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.TokenService.Infrastructure.OneTimePassword;
using Simonbu11.Otp;
using Simonbu11.Otp.Totp;

namespace SFA.DAS.TokenService.Infrastructure.UnitTests.OneTimePassword;

public class HmacSha512Tests
{
    [TestCase("GQ2TCMRSGQ2TELJRMRQTQLJUMI3DSLJYMYZWILJWGUYWMNBSMM3DSODDGY2DKMJSGI2DKMRNGFSGCOBNGRRDMOJNHBTDGZBNGY2TCZQ", 59, "29303361")]
    [TestCase("GQ2TCMRSGQ2TELJRMRQTQLJUMI3DSLJYMYZWILJWGUYWMNBSMM3DSODDGY2DKMJSGI2DKMRNGFSGCOBNGRRDMOJNHBTDGZBNGY2TCZQ", 1234567890, "84853786")]
    [TestCase("GQ2TCMRSGQ2TELJRMRQTQLJUMI3DSLJYMYZWILJWGUYWMNBSMM3DSODDGY2DKMJSGI2DKMRNGFSGCOBNGRRDMOJNHBTDGZBNGY2TCZQ", 2000000000, "98909859")]
    public void ThenItShouldGenerateCodeFromBase32Encoded(string sharedSecret, long secondsSinceEpoch, string expectedCode)
    {
        // Arrange
        var generator = new HmacSha512TotpGenerator(new TotpGeneratorSettings
        {
            SharedSecret = OtpSharedSecret.FromBase32String(sharedSecret)
        });
        
        var time = new DateTime(1970, 1, 1).AddSeconds(secondsSinceEpoch);

        // Act
        var actual = generator.Generate(time);

        // Assert
        actual.Should().Be(expectedCode);
    }
}