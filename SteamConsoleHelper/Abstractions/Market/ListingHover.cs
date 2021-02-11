namespace SteamConsoleHelper.Abstractions.Market
{
    public readonly struct ListingHover
    {
        public ulong ListingId { get; }

        public uint AppId { get; }

        public uint ContextId { get; }

        public ulong AssetId { get; }

        public ListingHover(ulong listingId, uint appId, uint contextId, ulong assetId)
        {
            ListingId = listingId;
            AppId = appId;
            ContextId = contextId;
            AssetId = assetId;
        }
    }
}