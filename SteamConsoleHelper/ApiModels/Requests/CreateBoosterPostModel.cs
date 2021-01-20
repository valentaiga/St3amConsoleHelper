namespace SteamConsoleHelper.ApiModels.Requests
{
    public class CreateBoosterPostModel
    {
        public string SessionId { get; set; }

        public uint AppId { get; set; }

        public uint Series { get; set; } = 1;

        // ReSharper disable once InconsistentNaming
        public uint Tradablity_preference { get; set; } = 3;

        public CreateBoosterPostModel(string sessionId, uint appId)
            => (SessionId, AppId) = (sessionId, appId);
    }
}