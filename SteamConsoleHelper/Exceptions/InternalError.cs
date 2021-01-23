namespace SteamConsoleHelper.Exceptions
{
    public enum InternalError
    {
        UnexpectedError,

        RequestBadRequest,
        RequestUnauthorized,
        TooManyRequests,
        RequestNotFound,
        SteamServicesAreBusy,
        FailedToDeserializeResponse,

        FailedToGetGamesFromCraftList,
        FailedToAddGameToCraftList,
        FailedToRemoveGameFromCraftList,
        InventoryItemIsNotACard,
        UserIsNotAuthenticated
    }
}