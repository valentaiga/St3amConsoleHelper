using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SteamConsoleHelper.Abstractions.Cache;
using SteamConsoleHelper.Abstractions.Inventory;
using SteamConsoleHelper.Helpers;
using SteamConsoleHelper.Services;

namespace SteamConsoleHelper.Extensions
{
    public static class CacheExtensions
    {
        public static async Task AddSentItemToMarketToCacheAsync(this LocalCacheService cacheService, InventoryItem item, uint price)
        {
            var cacheModel = new ItemWithPrice(item, price);
            await cacheService.AddToSetAsync(CacheKeyHelper.SentItemsToMarketCacheKey, cacheModel);
        }

        public static async Task RemoveSentItemToMarketFromCacheAsync(this LocalCacheService cacheService, InventoryItem item, uint price)
        {
            var cacheModel = new ItemWithPrice(item, price);
            await cacheService.RemoveFromSetAsync(CacheKeyHelper.SentItemsToMarketCacheKey, cacheModel);
        }

        public static async Task<List<ItemWithPrice>> GetCachedSentItemsToMarket(this LocalCacheService cacheService)
        {
            var set = await cacheService.GetValuesFromSet<ItemWithPrice>(CacheKeyHelper.SentItemsToMarketCacheKey);
            return set.ToList();
        }
    }
}