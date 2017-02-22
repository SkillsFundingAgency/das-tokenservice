using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.TokenService.Domain.Services;

namespace SFA.DAS.TokenService.Infrastructure.Services
{
    public class InMemoryCacheProvider : ICacheProvider
    {
        public Task<object> GetAsync(string key)
        {
            return Task.FromResult(MemoryCache.Default.Get(key));
        }

        public Task SetAsync(string key, object value, DateTimeOffset? expiryTime = null)
        {
            MemoryCache.Default.Set(key, value, expiryTime ?? DateTimeOffset.MaxValue);
            return Task.FromResult<object>(null);
        }
    }
}
