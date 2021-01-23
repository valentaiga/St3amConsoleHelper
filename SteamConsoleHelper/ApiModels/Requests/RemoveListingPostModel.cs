namespace SteamConsoleHelper.ApiModels.Requests
{
    public class RemoveListingPostModel
    {
        public string SessionId { get; set; }

        public RemoveListingPostModel(string sessionId)
            => (SessionId) = (sessionId);
    }
}