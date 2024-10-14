using System.Globalization;

namespace SFA.DAS.TokenService.Infrastructure.OneTimePassword;

internal static class Extensions
{
    internal static byte[] HexStringToBytes(this string hexString)
    {
        if (hexString.Length % 2 != 0)
        {
            throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
        }

        var hexAsBytes = new byte[hexString.Length / 2];
        
        for (var index = 0; index < hexAsBytes.Length; index++)
        {
            var byteValue = hexString.Substring(index * 2, 2);
            hexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return hexAsBytes;
    }
}