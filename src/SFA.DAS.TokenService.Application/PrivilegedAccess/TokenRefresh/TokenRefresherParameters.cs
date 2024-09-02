namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;

public class TokenRefresherParameters {

    public TokenRefresherParameters()
    {
        TokenRefreshExpirationPercentage = MaximumTokenRefreshLeadTime;
        RetryInterval = new TimeSpan(0, 0, 10);
    }

    private int _tokenRefreshExpirationPercentage;

    // The maximum amount of lead time
    public const int MaximumTokenRefreshLeadTime = 80;

    // The minimum amount of lead time.
    public const int MinimumTokenRefreshLeadTime = 100;

    /// <summary>
    ///     The token will be refreshed automatically in the background when it's within this
    ///     percentage of being expired. 
    /// </summary>
    /// <remarks>
    ///     This must be between 80 and 100. It will be automatically adjusted to be within 
    ///     this range. The upper limit for lead time (i.e. 80) exists to prevent unwittingly setting to 
    ///     a low value resulting in overly eagre token refreshes.
    /// </remarks>
    public int TokenRefreshExpirationPercentage
    {
        get => _tokenRefreshExpirationPercentage;
        set => _tokenRefreshExpirationPercentage = Math.Max(MaximumTokenRefreshLeadTime, Math.Min(MinimumTokenRefreshLeadTime, value));
    }

    /// <summary>
    ///     The interval to leave between retrying failures. This is a wrapper around Polly retry, which will eventually give up. 
    ///     We must keep going until we get a result. 
    /// </summary>
    public TimeSpan RetryInterval { get; set; }
}