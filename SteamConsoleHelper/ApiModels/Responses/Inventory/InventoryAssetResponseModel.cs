namespace SteamConsoleHelper.ApiModels.Responses.Inventory
{
    public class InventoryAssetResponseModel
    {
        public uint AppId { get; set; }

        public uint ContextId { get; set; }

        public ulong AssetId { get; set; }

        public ulong ClassId { get; set; }

        public ulong InstanceId { get; set; }

        public uint Amount { get; set; }
    }
}