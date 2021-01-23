using System;

namespace SteamConsoleHelper.Abstractions.Market
{
    public class ListingDescription
    {
        public ulong ListingId { get; set; }

        public uint BuyerPrice { get; set; }

        public uint SellerPrice { get; set; }

        public DateTime MarketSellDate { get; set; }

        public string HashName { get; set; }
    }
}