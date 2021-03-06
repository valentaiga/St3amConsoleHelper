using System;
using System.Diagnostics;

namespace SteamConsoleHelper.Abstractions.Market
{
    [DebuggerDisplay("ListingId = {ListingId}, SellerPrice = {SellerPrice}, BuyerPrice = {BuyerPrice}, HashName = {HashName}")]
    public readonly struct MarketListing
    {
        public ulong ListingId { get; }

        public ulong AssetId { get; }

        public uint SellerPrice { get; }

        public uint BuyerPrice { get; }

        public DateTime SellDate { get; }

        public string HashName { get; }

        public uint AppId { get; }

        public uint ContextId { get; }

        public ulong ClassId { get; }

        public bool AwaitingConfirmation { get; }

        public MarketListing(ulong listingId, ulong assetId, uint sellerPrice, uint buyerPrice, DateTime sellDate, string hashName, uint appId, uint contextId, ulong classId, bool awaitingConfirmation)
        {
            ListingId = listingId;
            AssetId = assetId;
            SellerPrice = sellerPrice;
            BuyerPrice = buyerPrice;
            SellDate = sellDate;
            HashName = hashName;
            AppId = appId;
            ContextId = contextId;
            ClassId = classId;
            AwaitingConfirmation = awaitingConfirmation;
        }
    }
}