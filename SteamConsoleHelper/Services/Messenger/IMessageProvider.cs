using System.Threading.Tasks;

namespace SteamConsoleHelper.Services.Messenger
{
    public interface IMessageProvider
    {
        Task SendMessageAsync(string message);

        Task<string> ReadMessageAsync();
    }
}