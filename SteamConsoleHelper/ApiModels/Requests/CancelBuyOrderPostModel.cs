namespace SteamConsoleHelper.ApiModels.Requests
{
    // ReSharper disable InconsistentNaming
    public class CancelBuyOrderPostModel
    {
        public string SessionId { get; set; }

        public ulong Buy_orderId { get; set; }

        public CancelBuyOrderPostModel(string sessionId, ulong buyOrderId)
            => (SessionId, Buy_orderId) = (sessionId, buyOrderId);
    }
}