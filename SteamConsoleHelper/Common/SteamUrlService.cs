using System.Net;

using SteamConsoleHelper.Resources;

namespace SteamConsoleHelper.Common
{
    public class SteamUrlService
    {
        private const string GetGemsForItemBaseUrl = "https://steamcommunity.com/auction/ajaxgetgoovalueforitemtype/?appid={0}&item_type={1}&border_color=0";
        private const string GrindSackToGemsBaseUrl = "https://steamcommunity.com/id/{0}/ajaxexchangegoo/";
        private const string GrindItemIntoGemsBaseUrl = "https://steamcommunity.com/id/{0}/ajaxgrindintogoo/";
        private const string UnpackBoosterBaseUrl = "https://steamcommunity.com/id/{0}/ajaxunpackbooster/";
        private const string BoosterCreatorBaseUrl = "https://steamcommunity.com/tradingcards/boostercreator/";
        private const string CreateBoosterBaseUrl = "https://steamcommunity.com/tradingcards/ajaxcreatebooster/";
        private const string GetInventoryBaseUrl = "https://steamcommunity.com/inventory/{0}/753/6?l=english&count=5000";
        private const string SellItemBaseUrl = "https://steamcommunity.com/market/sellitem/";
        private const string GetItemPriceBaseUrl = "https://steamcommunity.com/market/priceoverview/?appid={0}&country=RU&currency=5&market_hash_name={1}";
        private const string GetMarketListingsBaseUrl = "https://steamcommunity.com/market/mylistings?count=100&start={0}&l=english";
        private const string RemoveListingBaseUrl = "https://steamcommunity.com/market/removelisting/{0}";
        private const string GetNotificationsCountBaseUrl = "https://steamcommunity.com/actions/GetNotificationCounts";
        private const string GetMarketItemListingBaseUrl = "https://steamcommunity.com/market/listings/{0}/{1}";
        private const string CreateBuyOrderBaseUrl = "https://steamcommunity.com/market/createbuyorder/";
        private const string CancelBuyOrderBaseUrl = "https://steamcommunity.com/market/cancelbuyorder/";
        
        public string GetGemsForItemUrl(uint appId, ulong marketItemType) => string.Format(GetGemsForItemBaseUrl, appId, marketItemType);

        public string GrindSackToGemsUrl() => string.Format(GrindSackToGemsBaseUrl, Settings.UrlNickname);

        public string UnpackBoosterUrl() => string.Format(UnpackBoosterBaseUrl, Settings.UrlNickname);

        public string BoosterCreatorUrl() => BoosterCreatorBaseUrl;

        public string CreateBoosterUrl() => CreateBoosterBaseUrl;

        public string GrindItemIntoGemsUrl() => string.Format(GrindItemIntoGemsBaseUrl, Settings.UrlNickname);

        public string GetItemPriceUrl(uint appId, string marketHashName) => string.Format(GetItemPriceBaseUrl, appId, WebUtility.UrlEncode(marketHashName));

        public string SellItemUrl() => SellItemBaseUrl;

        public string GetMyListingsUrl(uint offset) => string.Format(GetMarketListingsBaseUrl, offset);

        public string RemoveListingUrl(ulong listingId) => string.Format(RemoveListingBaseUrl, listingId);

        public string GetCurrentInventoryUrl() => string.Format(GetInventoryBaseUrl, Settings.SteamId);

        public string GetNotificationsCountUrl() => GetNotificationsCountBaseUrl;

        public string GetMarketItemListingUrl(uint appId, string marketHashName) => string.Format(GetMarketItemListingBaseUrl, appId, WebUtility.UrlEncode(marketHashName));

        public string CreateBuyOrderUrl() => CreateBuyOrderBaseUrl;

        public string CancelBuyOrderUrl() => CancelBuyOrderBaseUrl;
    }
}