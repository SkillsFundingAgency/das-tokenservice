namespace SFA.DAS.TokenService.Infrastructure.OneTimePassword;

public class TotpGeneratorSettings : OtpGeneratorSettings
{
    public int TimeStepInterval { get; set; } = 30;
}

public abstract class OtpGeneratorSettings
{
    public OtpSharedSecret? SharedSecret { get; set; }
    public int CodeLength { get; set; } = 8;
}

public class OtpSharedSecret
{
    public byte[] Data { get; set; }
    
    public OtpSharedSecret(byte[] data)
    {
        Data = data;   
    }
    
    public byte[] GetKeyOfLength(int requiredLength)
    {
        if (Data.Length >= requiredLength)
        {
            return Data;
        }

        var buffer = new byte[requiredLength];
        var offset = 0;
        
        while (offset < buffer.Length)
        {
            var copyLength = offset + Data.Length > buffer.Length
                ? buffer.Length - offset
                : Data.Length;
            
            Array.Copy(Data, 0, buffer, offset, copyLength);
            offset += Data.Length;
        }

        return buffer;
    }
    
    public static OtpSharedSecret FromBase32String(string value)
    {
        return new OtpSharedSecret(Base32.ToBytes(value));
    }
}