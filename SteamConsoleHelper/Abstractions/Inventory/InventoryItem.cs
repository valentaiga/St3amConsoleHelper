using System.Collections.Generic;
using System.Diagnostics;
using SteamConsoleHelper.Abstractions.Enums;

namespace SteamConsoleHelper.Abstractions.Inventory
{
    [DebuggerDisplay("Name = {Name}, Amount = {Amount}, MarketName = {MarketName}, ItemType = {ItemType}, AssetId = {AssetId}")]
    public readonly struct InventoryItem
    {
        public string Name { get; }

        public string MarketName { get; }

        public string MarketHashName { get; }

        public uint AppId { get; }

        public uint ContextId { get; }

        public ulong AssetId { get; }

        public ulong ClassId { get; }

        public ulong InstanceId { get; }

        public uint Amount { get; }

        public string Type { get; }

        public string IconUrl { get; }

        public bool Tradable { get; }

        public bool Marketable { get; }

        public bool Commodity { get; }

        public ItemType ItemType { get; }

        public uint MarketFeeApp { get; }

        public uint? MarketItemType { get; }

        public List<ItemTag> Tags { get; }

        public InventoryItem(string name, string marketName, string marketHashName, uint appId, uint contextId, ulong assetId, ulong classId, ulong instanceId, uint amount, string type, string iconUrl, bool tradable, bool marketable, bool commodity, ItemType itemType, uint marketFeeApp, uint? marketItemType, List<ItemTag> tags)
        {
            Name = name;
            MarketName = marketName;
            MarketHashName = marketHashName;
            AppId = appId;
            ContextId = contextId;
            AssetId = assetId;
            ClassId = classId;
            InstanceId = instanceId;
            Amount = amount;
            Type = type;
            IconUrl = "https://community.cloudflare.steamstatic.com/economy/image/" + iconUrl;
            Tradable = tradable;
            Marketable = marketable;
            Commodity = commodity;
            ItemType = itemType;
            MarketFeeApp = marketFeeApp;
            MarketItemType = marketItemType;
            Tags = tags;
        }
    }
}