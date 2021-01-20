using SteamConsoleHelper.Resources;

namespace SteamConsoleHelper.Common
{
    public class SteamUrlService
    {
        // 0 - steamId
        private const string GetGemsForItemBaseUrl = "https://steamcommunity.com/auction/ajaxgetgoovalueforitemtype/?appid={0}&item_type={1}&border_color=0";
        private const string GrindItemIntoGooBaseUrl = "https://steamcommunity.com/id/{0}/ajaxgrindintogoo/";
        private const string UnpackBoosterBaseUrl = "https://steamcommunity.com/id/{0}/ajaxunpackbooster/";
        private const string CreateBoosterBaseUrl = "https://steamcommunity.com/tradingcards/ajaxcreatebooster/";
        private const string GetInventoryBaseUrl = "https://steamcommunity.com/inventory/{0}/753/6?l=english&count=5000";
        private const string SellItemBaseUrl = "https://steamcommunity.com/market/sellitem/";
        private const string GetItemPriceBaseUrl = "https://steamcommunity.com/market/priceoverview/?appid={0}&country=RU&currency=5&market_hash_name={1}";
        private const string GetMarketListingsBaseUrl = "https://steamcommunity.com/market/mylistings?count=100&start={0}";
        private const string RemoveListingBaseUrl = "https://steamcommunity.com/market/removelisting/{0}";

        private readonly ProfileSettings _profileSettings;

        public SteamUrlService(ProfileSettings profileSettings)
        {
            _profileSettings = profileSettings;
        }

        public string GetGemsForItemUrl(uint appId, uint itemType) => string.Format(GetGemsForItemBaseUrl, appId, itemType);

        public string UnpackBoosterUrl() => string.Format(UnpackBoosterBaseUrl, _profileSettings.SteamUrlNickname);

        public string CreateBoosterUrl() => CreateBoosterBaseUrl;

        public string GrindItemIntoGooUrl() => string.Format(GrindItemIntoGooBaseUrl, _profileSettings.SteamUrlNickname);

        public string GetItemPriceUrl(uint appId, string hashName) => string.Format(GetItemPriceBaseUrl, appId, hashName);

        public string SellItemUrl() => SellItemBaseUrl;

        public string GetMyListingsUrl(uint offset) => string.Format(GetMarketListingsBaseUrl, offset);

        public string RemoveListingUrl(ulong listingId) => string.Format(RemoveListingBaseUrl, listingId);

        public string GetCurrentInventoryUrl() => string.Format(GetInventoryBaseUrl, _profileSettings.SteamId);
    }
}