using System.Diagnostics;

namespace SteamConsoleHelper.Abstractions.BoosterPack
{
    [DebuggerDisplay("Name = {Name}, AppId = {AppId}, Price = {Price}")]
    public readonly struct BoosterGame
    {
        public uint AppId { get; }

        public string Name { get; }

        public int Series { get; }

        public uint Price { get; }

        public string ImageUrl => $"https://steamcdn-a.akamaihd.net/steam/apps/{AppId}/header.jpg";

        public BoosterGame(uint appId, string name, int series, uint price)
        {
            AppId = appId;
            Name = name;
            Series = series;
            Price = price;
        }
    }
}