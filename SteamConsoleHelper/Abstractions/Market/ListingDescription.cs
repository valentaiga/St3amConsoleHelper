using System;

namespace SteamConsoleHelper.Abstractions.Market
{
    public readonly struct ListingDescription
    {
        public ulong ListingId { get; }

        public uint BuyerPrice { get; }

        public uint SellerPrice { get; }

        public DateTime MarketSellDate { get; }

        public string HashName { get; }

        public bool AwaitingConfirmation { get; }

        public ListingDescription(ulong listingId, uint buyerPrice, uint sellerPrice, DateTime marketSellDate, string hashName, bool awaitingConfirmation)
        {
            ListingId = listingId;
            BuyerPrice = buyerPrice;
            SellerPrice = sellerPrice;
            MarketSellDate = marketSellDate;
            HashName = hashName;
            AwaitingConfirmation = awaitingConfirmation;
        }

        public static ListingDescription Empty
            => new ListingDescription(0, 0, 0, DateTime.MinValue, null, false);
    }
}