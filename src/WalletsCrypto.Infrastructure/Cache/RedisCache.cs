using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;
using WalletsCrypto.Common.Extensions;

namespace WalletsCrypto.Infrastructure.Cache
{
    public class RedisCache : ICacheStorage
    {
        private readonly IDistributedCache _cache;
        public RedisCache(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task<T> RetrieveAsync<T>(string key)
        {
            return (T)Convert.ChangeType(await _cache.GetAsync(key), typeof(T));
        }

        public async Task<string> RetrieveAsync(string key)
        {
            return await _cache.GetStringAsync(key);
        }

        public async Task StoreAsync(string key, object data)
        {
            await _cache.SetAsync(key, data.SerializeToByteArray());
        }

        public async Task StoreAsync(string key, string data)
        {
            await _cache.SetStringAsync(key, data);
        }

        public async Task StoreAsync(string key, string data, TimeSpan slidingExpiration)
        {
            await _cache.SetStringAsync(key, data, new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration });
        }

        public async Task StoreAsync(string key, object data, TimeSpan slidingExpiration)
        {
            await _cache.SetAsync(key, data.SerializeToByteArray(), new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration });
        }

        public async Task StoreAsync(string key, object data, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            await _cache.SetAsync(key, data.SerializeToByteArray(),
                new DistributedCacheEntryOptions { AbsoluteExpiration = absoluteExpiration, SlidingExpiration = slidingExpiration });
        }

        public async Task StoreAsync(string key, string data, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            await _cache.SetStringAsync(key, data,
                new DistributedCacheEntryOptions { AbsoluteExpiration = absoluteExpiration, SlidingExpiration = slidingExpiration });
        }
    }
}
