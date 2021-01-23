using System;
using SteamConsoleHelper.Abstractions.Inventory;

namespace SteamConsoleHelper.Abstractions.Cache
{
    public readonly struct ItemWithPrice
    {
        public InventoryItem Item { get; }

        public uint Price { get; }

        public DateTime SellTime { get; }

        public ItemWithPrice(InventoryItem item, uint price)
            => (Item, Price, SellTime) = (item, price, DateTime.UtcNow);
    }
}