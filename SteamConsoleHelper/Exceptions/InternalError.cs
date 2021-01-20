namespace SteamConsoleHelper.Exceptions
{
    public enum InternalError
    {
        RequestBadRequest,
        FailActionResult,
        FailedToDeserializeResponse,
        FailedToGetGamesFromCraftList,
        FailedToAddGameToCraftList,
        FailedToRemoveGameFromCraftList,
        InventoryItemIsNotACard,
    }
}