using System.Threading.Tasks;

using SteamAuth;

using SteamConsoleHelper.Storage.Models;

namespace SteamConsoleHelper.Storage
{
    public class FakeDataStoreService : IDataStoreService
    {
        public Task SaveCredentialsAsync(UserLogin userLogin)
        {
            return Task.CompletedTask;
        }

        public Task SaveTelegramChat(long chatId)
        {
            return Task.CompletedTask;
        }

        public Task<DataBlob> LoadJsonBlobAsync()
        {
            return Task.FromResult(new DataBlob());
        }
    }
}