using System.Threading.Tasks;

using SteamAuth;

namespace SteamConsoleHelper.Storage
{
    public interface IDataStoreService
    {
        Task SaveCredentialsAsync(UserLogin userLogin);

        Task<long?> GetTelegramChatIdAsync();

        Task SaveTelegramChatIdAsync(long chatId);

        Task<UserLogin> GetCredentialsAsync();
    }
}