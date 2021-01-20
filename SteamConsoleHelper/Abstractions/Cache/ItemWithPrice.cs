using SteamConsoleHelper.Abstractions.Inventory;

namespace SteamConsoleHelper.Abstractions.Cache
{
    public struct ItemWithPrice
    {
        public InventoryItem Item { get; set; }

        public uint Price { get; set; }

        public ItemWithPrice(InventoryItem item, uint price)
            => (Item, Price) = (item, price);
    }
}