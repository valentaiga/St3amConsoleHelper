namespace SteamConsoleHelper.Abstractions.BoosterPack
{
    public class BoosterCard
    {
        public string Name { get; set; }
        
        public bool IsFoil { get; set; }
        
        public string ImageUrl { get; set; }
        
        public uint Series { get; set; }
    }
}