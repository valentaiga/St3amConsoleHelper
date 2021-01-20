using System;
using System.Linq;
using System.Text.RegularExpressions;
using SteamConsoleHelper.Abstractions.BoosterPack;
using SteamConsoleHelper.Abstractions.Inventory;
using SteamConsoleHelper.Abstractions.Market;
using SteamConsoleHelper.ApiModels.Responses.BoosterPack;
using SteamConsoleHelper.ApiModels.Responses.Inventory;
using SteamConsoleHelper.ApiModels.Responses.Market;

namespace SteamConsoleHelper.Helpers
{
    public static class ModelMapper
    {
        private static readonly Regex NotDigitsRegex = new Regex(@"[^0-9]+");
        private static readonly Regex PriceRegex = new Regex(@"([0-9])+(,)?([0-9]{0,2})");

        public static ItemMarketPrice ToModel(this ItemPriceResponseModel response, uint appId, string hashName)
        {
            return new ItemMarketPrice
            {
                LowestPrice = ConvertStringToPrice(response.LowestPriceString),
                MedianPrice = ConvertStringToPrice(response.MedianPriceString),
                Volume = Convert.ToUInt32(NotDigitsRegex.Replace(response.Volume, string.Empty)),
                AppId = appId,
                HashName = hashName
            };

            static uint? ConvertStringToPrice(string value)
            {
                if (value == null)
                {
                    return null;
                }

                var stringPrice = PriceRegex.Match(value).Value;
                var stringPriceNoDot = NotDigitsRegex.Replace(stringPrice, string.Empty);

                var result = Convert.ToUInt32(stringPriceNoDot);

                if (!value.Contains(','))
                {
                    result *= 100;
                }

                return result;
            }
        }

        public static InventoryItem ToModel(this InventoryAssetResponseModel asset, InventoryDescriptionResponseModel description)
        {
            var tagName = description.Tags?.Find(x => x.Category == "item_class")?.LocalizedTagName;
            var type = ItemTypeIdentifier.ParseTypeFromDescription(tagName);

            return new InventoryItem
            {
                AssetId = asset.AssetId,
                AppId = asset.AppId,
                ClassId = asset.ClassId,
                ContextId = asset.ContextId,
                Amount = asset.Amount,
                MarketName = description.MarketName,
                InstanceId = asset.InstanceId,
                Commodity = description.Commodity,
                Marketable = description.Marketable,
                Tradable = description.Tradable,
                Name = description.Name,
                MarketHashName = description.MarketHashName,
                Type = description.Type,
                ItemType = type,
                IconUrl = "https://community.cloudflare.steamstatic.com/economy/image/" + description.IconUrl,
                Tags = description.Tags.Select(x => x.ToModel()).ToList()
            };
        }

        public static MarketListing ToModel(this InventoryAssetResponseModel asset)
        {
            return new MarketListing
            {
                AppId = asset.AppId,
                AssetId = asset.AssetId,
                ClassId = asset.ClassId,
                ContextId = asset.ContextId,
                InstanceId = asset.InstanceId
            };
        }

        private static ItemTag ToModel(this ItemTagResponseModel response)
        {
            return new ItemTag
            {
                Name = response.InternalName,
                LocalizedTagName = response.LocalizedTagName,
                Category = response.Category,
                LocalizedCategoryName = response.LocalizedCategoryName
            };
        }

        public static BoosterCard ToModel(this BoosterCardResponseModel response)
        {
            return new BoosterCard
            {
                Name = response.Name,
                ImageUrl = response.ImageUrl,
                IsFoil = response.IsFoil,
                Series = response.Series
            };
        }
    }
}