namespace SteamConsoleHelper.ApiModels.Requests
{
    public class UnpackBoosterPostModel
    {
        public string SessionId { get; set; }

        public uint AppId { get; set; }

        public ulong CommunityItemId { get; set; }

        public UnpackBoosterPostModel(string sessionId, uint appId, ulong assetId)
            => (SessionId, AppId, CommunityItemId) = (sessionId, appId, assetId);
    }
}