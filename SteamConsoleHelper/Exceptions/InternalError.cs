namespace SteamConsoleHelper.Exceptions
{
    public enum InternalError
    {
        RequestBadRequest,
        SteamServicesAreBusy,
        UnexpectedError,
        FailedToDeserializeResponse,
        FailedToGetGamesFromCraftList,
        FailedToAddGameToCraftList,
        FailedToRemoveGameFromCraftList,
        InventoryItemIsNotACard,
    }
}