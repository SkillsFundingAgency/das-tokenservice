namespace SFA.DAS.TokenService.Infrastructure.OneTimePassword;

public abstract class TotpGenerator : OtpGenerator
{
    private readonly DateTime _epoch;
    private readonly TotpGeneratorSettings _settings;

    protected TotpGenerator(TotpGeneratorSettings settings) : base(settings)
    {
        _settings = settings;
        _epoch = new DateTime(1970, 1, 1);
    }

    public string Generate(DateTime time)
    {
        var timeStep = (long)Math.Floor((time - _epoch).TotalSeconds / _settings.TimeStepInterval);
        return Generate(timeStep);
    }
    
    private string Generate(long timeStep)
    {
        var time = timeStep.ToString("X");
        while (time.Length < 16)
        {
            time = "0" + time;
        }

        // Get the HEX in a Byte[]
        var msg = time.HexStringToBytes();

        return Generate(msg);
    }
}