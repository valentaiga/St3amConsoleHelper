using System.Threading.Tasks;

using SteamAuth;

using SteamConsoleHelper.Storage.Models;

namespace SteamConsoleHelper.Storage
{
    public interface IDataStoreService
    {
        Task SaveCredentialsAsync(UserLogin userLogin);
        
        Task SaveTelegramChat(long chatId);

        Task<DataBlob> LoadJsonBlobAsync();
    }
}