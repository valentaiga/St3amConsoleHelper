using System.Threading;
using System.Threading.Tasks;

namespace SteamConsoleHelper.Services.Messenger
{
    public interface IMessageProvider
    {
        Task SendMessageAsync(string message, CancellationToken stoppingToken);

        Task<string> ReadMessageAsync(CancellationToken stoppingToken);
    }
}