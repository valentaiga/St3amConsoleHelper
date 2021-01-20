using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamConsoleHelper.Services
{
    public class LocalCacheService
    {
        private readonly ConcurrentDictionary<string, object> _cache;

        public LocalCacheService()
        {
            _cache = new ConcurrentDictionary<string, object>();
        }

        public T Get<T>(string cacheKey)
        {
            _cache.TryGetValue(cacheKey, out var value);
                
            return (T)value;
        }

        public async Task<T> GetAsync<T>(string cacheKey)
        {
            return await Task.Run(() => Get<T>(cacheKey));
        }

        public void Set<T>(string cacheKey, T value)
        {
            _cache.AddOrUpdate(cacheKey, (_) => value, (_, __) => value);
        }

        public async Task SetAsync<T>(string cacheKey, T value)
        {
            await Task.Run(() => Set(cacheKey, value));
        }

        public async Task AddToSetAsync<T>(string cacheKey, T value)
        {
            await Task.Run(() =>
            {
                _cache.AddOrUpdate(cacheKey,
                    (_) => new HashSet<T> {value},
                    (_, existingSet) =>
                    {
                        var set = (HashSet<T>) existingSet;
                        set.Add(value);
                        return set;
                    });
            });
        }

        public async Task RemoveFromSetAsync<T>(string cacheKey, T value)
        {
            var set = await GetAsync<HashSet<T>>(cacheKey);

            if (set == null)
            {
                return;
            }

            set.Remove(value);

            await SetAsync(cacheKey, set);
        }

        public async Task<HashSet<T>> GetValuesFromSet<T>(string cacheKey)
        {
            var result = await GetAsync<HashSet<T>>(cacheKey);

            return result ?? new HashSet<T>();
        }
    }
}