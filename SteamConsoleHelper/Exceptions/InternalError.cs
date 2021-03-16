namespace SteamConsoleHelper.Exceptions
{
    public enum InternalError
    {
        UnexpectedError,
        UnexpectedRequestError,

        RequestBadRequest,
        RequestUnauthorized,
        TooManyRequests,
        RequestNotFound,
        SteamServicesAreBusy,
        FailedToDeserializeResponse,

        InventoryItemIsNotACard,
        InventoryItemIsNotASackOfGems,
        UserIsNotAuthenticated
    }
}