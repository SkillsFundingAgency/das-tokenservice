namespace SFA.DAS.TokenService.Infrastructure.OneTimePassword;

public abstract class OtpGenerator(OtpGeneratorSettings settings)
{
    protected string Generate(byte[] msg)
    {
        var hashKey = ConvertSecretToHashKey(settings.SharedSecret);

        var hash = ComputeHash(hashKey, msg);

        // put selected bytes into result int
        var offset = hash[hash.Length - 1] & 0xf;

        var binary =
            ((hash[offset] & 0x7f) << 24) |
            ((hash[offset + 1] & 0xff) << 16) |
            ((hash[offset + 2] & 0xff) << 8) |
            (hash[offset + 3] & 0xff);

        var otp = binary % (int)Math.Pow(10, settings.CodeLength);

        var result = otp.ToString();
        while (result.Length < settings.CodeLength)
        {
            result = "0" + result;
        }
        return result;
    }

    protected abstract byte[] ConvertSecretToHashKey(OtpSharedSecret? sharedSecret);
    protected abstract byte[] ComputeHash(byte[] k, byte[] msg);
}