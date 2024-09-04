using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using SFA.DAS.EAS.Infrastructure.ExecutionPolicies;

namespace SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;

[PolicyName(Name)]
public class HmrcExecutionPolicy : ExecutionPolicy
{
    public const string Name = "HMRC Policy";
    private readonly ILogger<HmrcExecutionPolicy> _logger;

    public HmrcExecutionPolicy(ILogger<HmrcExecutionPolicy> logger)
    {
        _logger = logger;
        TimeSpan retryWaitTime = new(0, 0, 0, 10);
        Initialize(retryWaitTime);
    }
    
    public HmrcExecutionPolicy(ILogger<HmrcExecutionPolicy> logger, TimeSpan retryWaitTime)
    {
        _logger = logger;
        Initialize(retryWaitTime);
    }
    
    private void Initialize(TimeSpan retryWaitTime)
    {
        var tooManyRequestsPolicy = Policy.Handle<HttpRequestException>(ex => ex.StatusCode.Equals(HttpStatusCode.TooManyRequests)).WaitAndRetryForeverAsync(i => retryWaitTime, (ex, ts) => OnRetryableFailure(ex));
        var requestTimeoutPolicy = Policy.Handle<HttpRequestException>(ex => ex.StatusCode.Equals(HttpStatusCode.RequestTimeout)).WaitAndRetryForeverAsync(i => retryWaitTime, (ex, ts) => OnRetryableFailure(ex));
        var serviceUnavailablePolicy = CreateAsyncRetryPolicy<HttpRequestException>(ex => ex.StatusCode.Equals(HttpStatusCode.ServiceUnavailable), 5, retryWaitTime, OnRetryableFailure);
        var internalServerErrorPolicy = CreateAsyncRetryPolicy<HttpRequestException>(ex => ex.StatusCode.Equals(HttpStatusCode.InternalServerError), 5, retryWaitTime, OnRetryableFailure);

        RootPolicy = Policy.WrapAsync(tooManyRequestsPolicy, serviceUnavailablePolicy, internalServerErrorPolicy, requestTimeoutPolicy);
    }

    protected override T? OnException<T>(Exception ex) where T : default
    {
        if (ex is HttpRequestException httpRequestException)
        {
            _logger.LogInformation("HttpRequestException - {HttpRequestException}", httpRequestException.ToString());

            if (httpRequestException.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Resource not found - {HttpRequestException}", httpRequestException.ToString());
                return default;
            }
        }

        _logger.LogError(ex, "Exceeded retry limit - {Ex}", ex.ToString());
        throw ex;
    }

    private void OnRetryableFailure(Exception ex)
    {
        _logger.LogInformation("Error calling HMRC - {Ex} - Will retry", ex.ToString());
    }
}