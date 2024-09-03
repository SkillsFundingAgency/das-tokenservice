using System.Security.Cryptography;

namespace SFA.DAS.TokenService.Infrastructure.OneTimePassword;

public class HmacSha512TotpGenerator(TotpGeneratorSettings settings) : TotpGenerator(settings)
{
    protected override byte[] ConvertSecretToHashKey(OtpSharedSecret sharedSecret)
    {
        return sharedSecret.GetKeyOfLength(64);
    }

    protected override byte[] ComputeHash(byte[] k, byte[] msg)
    {
        return new HMACSHA512(k).ComputeHash(msg);
    }
}