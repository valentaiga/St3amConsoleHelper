namespace SteamConsoleHelper.ApiModels.Requests
{
    public class GrindItemIntoGemsPostModel
    {
        public string SessionId { get; set; }

        public uint AppId { get; set; }

        public ulong AssetId { get; set; }

        public uint ContextId { get; set; }

        // ReSharper disable once InconsistentNaming
        public uint Goo_value_expected { get; } = 100;

        public GrindItemIntoGemsPostModel(string sessionId, uint appId, ulong assetId, uint contextId)
            => (SessionId, AppId, AssetId, ContextId) = (sessionId, appId, assetId, contextId);
    }
}