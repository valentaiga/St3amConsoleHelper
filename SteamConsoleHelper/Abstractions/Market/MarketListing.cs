using System;

namespace SteamConsoleHelper.Abstractions.Market
{
    public class MarketListing
    {
        public ulong ListingId { get; set; }

        public ulong AssetId { get; set; }

        public uint SellerPrice { get; set; }

        public uint BuyerPrice { get; set; }

        public DateTime SellDate { get; set; }

        public string HashName { get; set; }

        public uint AppId { get; set; }

        public uint ContextId { get; set; }

        public ulong ClassId { get; set; }
    }
}