namespace SFA.DAS.TokenService.Infrastructure.OneTimePassword;

public abstract class TotpGenerator(TotpGeneratorSettings settings) : OtpGenerator(settings)
{
    private readonly DateTime _epoch = DateTime.UnixEpoch;

    public string Generate(DateTime time)
    {
        var timeStep = (long)Math.Floor((time - _epoch).TotalSeconds / settings.TimeStepInterval);
        return Generate(timeStep);
    }

    private string Generate(long timeStep)
    {
        var time = timeStep.ToString("X");
        while (time.Length < 16)
        {
            time = "0" + time;
        }

        var msg = time.HexStringToBytes();

        return Generate(msg);
    }
}