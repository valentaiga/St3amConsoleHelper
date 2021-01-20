using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SteamConsoleHelper.Exceptions;

namespace SteamConsoleHelper.Services
{
    public class StoreService
    {
        private readonly ConcurrentDictionary<StorageType, string> _cache;

        public StoreService()
        {
            // todo: Idk how to manage this service
            _cache = new ConcurrentDictionary<StorageType, string>();
            
            // just local initialization
            _cache.AddOrUpdate(
                StorageType.CraftList, 
                "515040,1172470,814540,459820,1091500,899970,746650,333600,420110,602520,1406990,524220,407330,592660",
                (_, __) => null);

        }

        public async Task<uint[]> GetCraftListApps()
            => await Task.Run(() =>
            {
                if (!_cache.TryGetValue(StorageType.CraftList, out var appsArrayString))
                {
                    throw new InternalException(InternalError.FailedToGetGamesFromCraftList);
                }
                return appsArrayString?.Split(',').Select(x => Convert.ToUInt32(x)).ToArray() ?? new uint[0];
            });

        public async Task<bool> TryAddAppToCraftList(uint appId)
            => await Task.Run(() =>
            {
                if (!_cache.TryGetValue(StorageType.CraftList, out var appsArrayString))
                {
                    throw new InternalException(InternalError.FailedToAddGameToCraftList);
                }

                var appsList = appsArrayString?.Split(',').ToList() ?? new List<string>();
                appsList.Add(appId.ToString());
                var saveValue = string.Join(',', new HashSet<string>(appsList));

                _cache.AddOrUpdate(StorageType.CraftList, (_) => saveValue, (_, __) => saveValue);

                return true;
            });

        public async Task<bool> TryRemoveAppToCraftList(uint appId)
            => await Task.Run(() =>
            {
                if (!_cache.TryGetValue(StorageType.CraftList, out var appsArrayString))
                {
                    throw new InternalException(InternalError.FailedToAddGameToCraftList);
                }

                var appsList = appsArrayString?.Split(',').ToList() ?? new List<string>();
                appsList.Remove(appId.ToString());
                var saveValue = string.Join(',', new HashSet<string>(appsList));

                _cache.AddOrUpdate(StorageType.CraftList, (_) => saveValue, (_, __) => saveValue);

                return true;
            });

        private enum StorageType
        {
            CraftList,
        }
    }
}