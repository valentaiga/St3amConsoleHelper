using SteamAuth;

namespace SteamConsoleHelper.Storage.Models
{
    public class DataBlob
    {
        public UserLogin UserLogin { get; set; } = new UserLogin(null, null);

        public long? ChatId { get; set; } = null;
    }
}