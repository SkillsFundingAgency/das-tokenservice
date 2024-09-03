using SFA.DAS.TokenService.Domain.Services;
using Simonbu11.Otp;
using Simonbu11.Otp.Totp;

namespace SFA.DAS.TokenService.Infrastructure.Services;

public class TotpService : ITotpService
{
    public string Generate(string secret)
    {
        var generator = new UnpaddedHmacSha512TotpGenerator(new TotpGeneratorSettings
        {
            SharedSecret = OtpSharedSecret.FromBase32String(secret)
        });
        return generator.Generate(DateTime.UtcNow);
    }

    private class UnpaddedHmacSha512TotpGenerator(TotpGeneratorSettings settings) : HmacSha512TotpGenerator(settings)
    {
        protected override byte[] ConvertSecretToHashKey(OtpSharedSecret sharedSecret)
        {
            return sharedSecret.Data;
        }
    }
}