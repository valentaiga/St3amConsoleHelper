using System.Collections.Generic;
using System.Diagnostics;
using SteamConsoleHelper.Abstractions.Enums;

namespace SteamConsoleHelper.Abstractions.Inventory
{
    [DebuggerDisplay("Name = {Name}, Amount = {Amount}, MarketName = {MarketName}, ItemType = {ItemType}, AssetId = {AssetId}")]
    public class InventoryItem
    {
        public string Name { get; set; }

        public string MarketName { get; set; }

        public string MarketHashName { get; set; }

        public uint AppId { get; set; }

        public uint ContextId { get; set; }

        public ulong AssetId { get; set; }

        public ulong ClassId { get; set; }

        public ulong InstanceId { get; set; }

        public uint Amount { get; set; }

        public string Type { get; set; }

        public string IconUrl { get; set; }

        public bool Tradable { get; set; }

        public bool Marketable { get; set; }

        public bool Commodity { get; set; }

        public ItemType ItemType { get; set; }

        public List<ItemTag> Tags { get; set; }
    }
}