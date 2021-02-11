namespace SteamConsoleHelper.Abstractions.BoosterPack
{
    public readonly struct BoosterCard
    {
        public string Name { get; }
        
        public bool IsFoil { get; }
        
        public string ImageUrl { get; }
        
        public uint Series { get; }

        public BoosterCard(string name, bool isFoil, string imageUrl, uint series)
        {
            Name = name;
            IsFoil = isFoil;
            ImageUrl = imageUrl;
            Series = series;
        }
    }
}