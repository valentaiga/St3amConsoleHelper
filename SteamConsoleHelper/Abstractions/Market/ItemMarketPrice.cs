namespace SteamConsoleHelper.Abstractions.Market
{
    public readonly struct ItemMarketPrice
    {
        public uint AppId { get;}

        public string HashName { get; }

        public uint? LowestPrice { get; }

        public uint? MedianPrice { get; }

        public uint? Volume { get; }

        public ItemMarketPrice(uint appId, string hashName, uint? lowestPrice, uint? medianPrice, uint? volume)
        {
            AppId = appId;
            HashName = hashName;
            LowestPrice = lowestPrice;
            MedianPrice = medianPrice;
            Volume = volume;
        }
    }
}