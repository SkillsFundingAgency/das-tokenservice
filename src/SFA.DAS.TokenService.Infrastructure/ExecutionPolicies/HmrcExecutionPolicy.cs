using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using SFA.DAS.EAS.Infrastructure.ExecutionPolicies;

namespace SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;

[PolicyName(Name)]
public class HmrcExecutionPolicy : ExecutionPolicy
{
    public const string Name = "HMRC Policy";

    private readonly ILogger _logger;

    public HmrcExecutionPolicy(ILogger<HmrcExecutionPolicy> logger, TimeSpan retryWaitTime)
    {
        _logger = logger;

        var tooManyRequestsPolicy = Policy.Handle<HttpRequestException>(ex => ex.StatusCode.Equals(429)).WaitAndRetryForeverAsync(i => retryWaitTime, (ex, ts) => OnRetryableFailure(ex));
        var requestTimeoutPolicy = Policy.Handle<HttpRequestException>(ex => ex.StatusCode.Equals(408)).WaitAndRetryForeverAsync(i => retryWaitTime, (ex, ts) => OnRetryableFailure(ex));
        var serviceUnavailablePolicy = CreateAsyncRetryPolicy<HttpRequestException>(ex => ex.StatusCode.Equals(503), 5, retryWaitTime, OnRetryableFailure);
        var internalServerErrorPolicy = CreateAsyncRetryPolicy<HttpRequestException>(ex => ex.StatusCode.Equals(500), 5, retryWaitTime, OnRetryableFailure);

        RootPolicy = Policy.WrapAsync(tooManyRequestsPolicy, serviceUnavailablePolicy, internalServerErrorPolicy, requestTimeoutPolicy);
    }

    protected override T? OnException<T>(Exception ex) where T : default
    {
        if (ex is HttpRequestException exception)
        {
            _logger.LogInformation("ApiHttpException - {Ex}", ex);

            switch (exception.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    _logger.LogInformation("Resource not found - {Ex}", ex);
                    return default;
            }
        }

        _logger.LogError(ex, "Exceeded retry limit - {Ex}", ex);
        throw ex;
    }

    private void OnRetryableFailure(Exception ex)
    {
        _logger.LogInformation("Error calling HMRC - {Ex} - Will retry", ex);
    }
}