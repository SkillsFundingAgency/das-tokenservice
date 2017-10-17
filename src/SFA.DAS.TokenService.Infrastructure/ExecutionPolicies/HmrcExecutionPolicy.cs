using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Polly;
using SFA.DAS.EAS.Infrastructure.ExecutionPolicies;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.TokenService.Infrastructure.ExecutionPolicies
{
    [PolicyName(Name)]
    public class HmrcExecutionPolicy : ExecutionPolicy
    {
        public const string Name = "HMRC Policy";

        private readonly ILog _logger;
        private readonly Policy TooManyRequestsPolicy;
        private readonly Policy ServiceUnavailablePolicy;
        private readonly Policy InternalServerErrorPolicy;
        private readonly Policy RequestTimeoutPolicy;
        private readonly Policy UnauthorizedPolicy;

        public string RetryToken { get; set; }

        public HmrcExecutionPolicy(ILog logger)
        {
            _logger = logger;

            TooManyRequestsPolicy = Policy.Handle<HttpException>().WaitAndRetryForeverAsync((i) => new TimeSpan(0, 0, 10), (ex, ts) =>
            {
                if (((HttpException)ex).WebEventCode == 429)
                {
                    OnRetryableFailure(ex, 429, "Rate limit has been reached");
                }
            });
            RequestTimeoutPolicy = Policy.Handle<HttpException>().WaitAndRetryForeverAsync((i) => new TimeSpan(0, 0, 10), (ex, ts) =>
            {
                if (((HttpException)ex).WebEventCode == 408)
                {
                    OnRetryableFailure(ex, 408, "Request has time out");
                }
            });
            ServiceUnavailablePolicy = CreateAsyncRetryPolicy<HttpException>(5, new TimeSpan(0, 0, 10), (ex) =>
            {
                if (((HttpException)ex).WebEventCode == 503)
                {
                    OnRetryableFailure(ex, 503, "Service is unavailable");
                }
            });
            InternalServerErrorPolicy = CreateAsyncRetryPolicy<HttpException>(5, new TimeSpan(0, 0, 10), (ex) =>
            {
                if (((HttpException)ex).WebEventCode == 500)
                {
                    OnRetryableFailure(ex, 500, "Internal server error");
                }
            });
            UnauthorizedPolicy =
                CreateAsyncRetryPolicy<HttpException>(5, new TimeSpan(0, 0, 10), (ex) =>
                {
                    if (((HttpException)ex).WebEventCode == 401)
                    {
                        OnRetryableFailure(ex, 401, ex.Message);
                    }
                });
            RootPolicy = Policy.WrapAsync(TooManyRequestsPolicy, ServiceUnavailablePolicy, InternalServerErrorPolicy, RequestTimeoutPolicy, UnauthorizedPolicy);
        }

        protected override T OnException<T>(Exception ex)
        {
            _logger.Error(ex, $"Exceeded retry limit - {ex.Message}");
            return default(T);
        }

        private void OnRetryableFailure(Exception ex, int statusCode, string message)
        {
            _logger.Info($"Error calling HMRC - {ex.Message} - Will retry");
        }
    }
}
