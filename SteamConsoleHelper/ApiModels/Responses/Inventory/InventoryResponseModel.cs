using System.Collections.Generic;

namespace SteamConsoleHelper.ApiModels.Responses.Inventory
{
    public class InventoryResponseModel : SteamResponseBase
    {
        public List<InventoryAssetResponseModel> Assets { get; set; }

        public List<InventoryDescriptionResponseModel> Descriptions { get; set; }

        public uint TotalInventoryCount { get; set; }
    }
}