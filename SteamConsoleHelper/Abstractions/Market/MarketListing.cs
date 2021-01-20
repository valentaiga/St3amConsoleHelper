namespace SteamConsoleHelper.Abstractions.Market
{
    public class MarketListing
    {
        public uint AppId { get; set; }

        public uint ContextId { get; set; }

        public ulong AssetId { get; set; }

        public ulong ClassId { get; set; }

        public ulong InstanceId { get; set; }
    }
}