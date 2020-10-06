using System;
using System.Threading.Tasks;

namespace WalletsCrypto.Infrastructure.Cache
{
    public interface ICacheStorage 
    {
        Task RemoveAsync(string key);
        Task<T> RetrieveAsync<T>(string key);
        Task<string> RetrieveAsync(string key);
        Task StoreAsync(string key, object data);
        Task StoreAsync(string key, string data);
        Task StoreAsync(string key, string data, TimeSpan slidingExpiration);
        Task StoreAsync(string key, object data, TimeSpan slidingExpiration);
        Task StoreAsync(string key, object data, DateTime absoluteExpiration, TimeSpan slidingExpiration);
        Task StoreAsync(string key, string data, DateTime absoluteExpiration, TimeSpan slidingExpiration);
    }
}
