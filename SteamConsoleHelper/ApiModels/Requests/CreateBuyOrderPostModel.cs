namespace SteamConsoleHelper.ApiModels.Requests
{
    // ReSharper disable InconsistentNaming
    public class CreateBuyOrderPostModel
    {
        public string SessionId { get; set; }

        // const for rubles currency
        public uint Currency { get; set; } = 5;

        public uint Appid { get; set; }

        public string Market_hash_name { get; set; }

        public uint Price_total { get; set; }

        public uint Quantity { get; set; }

        public string Billing_state { get; set; } = string.Empty;

        public uint Save_my_address { get; set; } = 0;

        public CreateBuyOrderPostModel(string sessionId, uint appid, string marketHashName, uint priceTotal, uint quantity)
            => (SessionId, Appid, Market_hash_name, Price_total, Quantity) = (sessionId, appid, marketHashName, priceTotal, quantity);
    }
}