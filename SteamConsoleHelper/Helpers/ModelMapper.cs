using System.Linq;

using SteamConsoleHelper.Abstractions.BoosterPack;
using SteamConsoleHelper.Abstractions.Inventory;
using SteamConsoleHelper.Abstractions.Market;
using SteamConsoleHelper.ApiModels.Responses.BoosterPack;
using SteamConsoleHelper.ApiModels.Responses.Inventory;
using SteamConsoleHelper.ApiModels.Responses.Market;
using SteamConsoleHelper.Extensions;

namespace SteamConsoleHelper.Helpers
{
    public static class ModelMapper
    {
        public static ItemMarketPrice ToModel(this ItemPriceResponseModel response, uint appId, string hashName)
        {
            return new ItemMarketPrice(
                appId,
                hashName,
                ParseHelper.ParsePrice(response.LowestPriceString),
                ParseHelper.ParsePrice(response.MedianPriceString),
                response.Volume.KeepNumbersOnly()?.ToUInt());
        }

        public static InventoryItem ToModel(this InventoryAssetResponseModel asset, InventoryDescriptionResponseModel description)
        {
            var typeTagName = description.Tags?.Find(x => x.Category == "item_class")?.LocalizedTagName;
            var type = ItemTypeIdentifier.ParseTypeFromDescription(typeTagName, description.MarketName);

            var marketItemType = description.OwnerActions?.Find(x => x.Name == "Turn into Gems...")?.Link;
            var marketItemTypeParsed = ParseHelper.ParseMarketItemType(marketItemType);

            return new InventoryItem(
                description.Name,
                description.MarketName,
                description.MarketHashName,
                asset.AppId,
                asset.ContextId,
                asset.AssetId,
                asset.ClassId,
                asset.InstanceId,
                asset.Amount,
                description.Type,
                description.IconUrl,
                description.Tradable,
                description.Marketable,
                description.Commodity,
                type,
                description.MarketFeeApp,
                marketItemTypeParsed,
                description.Tags!.Select(x => x.ToModel()).ToList()
            );
        }

        public static MarketListing ToModel(this ListingAsset asset, ListingHover listingHover, ListingDescription listingDescription)
        {
            return new MarketListing(
                listingHover.ListingId,
                asset.AssetId,
                listingDescription.SellerPrice,
                listingDescription.BuyerPrice,
                listingDescription.MarketSellDate,
                listingDescription.HashName,
                asset.AppId,
                asset.ContextId,
                asset.ClassId,
                listingDescription.AwaitingConfirmation);
        }

        private static ItemTag ToModel(this InventoryDescriptionResponseModel.ItemTagResponseModel response)
        {
            return new ItemTag(
                response.InternalName,
                response.Category,
                response.LocalizedCategoryName,
                response.LocalizedTagName);
        }

        public static BoosterCard ToModel(this BoosterCardResponseModel response)
        {
            return new BoosterCard(
                response.Name,
                response.IsFoil,
                response.ImageUrl,
                response.Series);
        }
    }
}