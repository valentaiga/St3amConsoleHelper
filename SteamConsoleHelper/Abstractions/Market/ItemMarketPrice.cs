namespace SteamConsoleHelper.Abstractions.Market
{
    public class ItemMarketPrice
    {
        public uint AppId { get; set; }

        public string HashName { get; set; }

        public uint? LowestPrice { get; set; }

        public uint? MedianPrice { get; set; }

        public uint? Volume { get; set; }
    }
}