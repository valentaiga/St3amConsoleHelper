using System.Threading.Tasks;

using SteamAuth;

namespace SteamConsoleHelper.Storage
{
    public class FakeDataStoreService : IDataStoreService
    {
        public Task SaveCredentialsAsync(UserLogin userLogin)
        {
            return Task.CompletedTask;
        }

        public Task<long?> GetTelegramChatIdAsync()
        {
            return Task.FromResult<long?>(null);
        }

        public Task SaveTelegramChatIdAsync(long chatId)
        {
            return Task.CompletedTask;
        }

        public Task<UserLogin> GetCredentialsAsync()
        {
            return Task.FromResult(new UserLogin(null, null));
        }
    }
}