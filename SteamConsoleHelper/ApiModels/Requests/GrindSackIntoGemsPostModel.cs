namespace SteamConsoleHelper.ApiModels.Requests
{
    // ReSharper disable InconsistentNaming
    public class GrindSackIntoGemsPostModel
    {
        public string SessionId { get; set; }

        public uint AppId { get; set; }

        public ulong AssetId { get; set; }

        public uint Goo_denomination_in { get; set; } = 1000;

        public uint Goo_amount_in { get; set; }

        public uint Goo_denomination_out { get; set; } = 1;

        public uint Goo_amount_out_expected => 1000 * Goo_amount_in;

        public GrindSackIntoGemsPostModel(string sessionId, uint appId, ulong assetId, uint gooAmountToOpen)
            => (SessionId, AppId, AssetId, Goo_amount_in) = (sessionId, appId, assetId, gooAmountToOpen);
    }
}