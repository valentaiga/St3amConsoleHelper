namespace SteamConsoleHelper.ApiModels.Requests
{
    public class SellItemPostModel
    {
        public string SessionId { get; set; }

        public uint AppId { get; set; }
        
        public uint ContextId { get; set; }

        public ulong AssetId { get; set; }

        public uint Amount { get; } = 1;

        public uint Price { get; set; }

        public SellItemPostModel(string sessionId, uint appId, ulong assetId, uint contextId, uint price)
            => (SessionId, AppId, ContextId, AssetId, Price) = (sessionId, appId, contextId, assetId, price);
    }
}