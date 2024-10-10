namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;

public static class DateTimeExtensions 
{
    public static TimeSpan GetPercentageTowards(this DateTime minTime, DateTime maxTime, int requiredLeadTime)
    {
        if (minTime >= maxTime)
        {
            return TimeSpan.Zero;
        }

        var difference = (maxTime - minTime).Ticks;
        var requiredTimeSpan = new TimeSpan(difference * requiredLeadTime / 100);
        return requiredTimeSpan;
    }
}