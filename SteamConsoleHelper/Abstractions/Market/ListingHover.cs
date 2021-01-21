namespace SteamConsoleHelper.Abstractions.Market
{
    public class ListingHover
    {
        public ulong ListingId { get; set; }

        public uint AppId { get; set; }

        public uint ContextId { get; set; }

        public ulong AssetId { get; set; }
    }
}