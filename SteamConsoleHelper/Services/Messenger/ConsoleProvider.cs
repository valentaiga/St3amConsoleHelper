using System;
using System.Threading;
using System.Threading.Tasks;

namespace SteamConsoleHelper.Services.Messenger
{
    public class ConsoleProvider : IMessageProvider
    {
        public Task SendMessageAsync(string message, CancellationToken stoppingToken)
        {
            return Task.Run(() => Console.WriteLine(message), stoppingToken);
        }

        public Task<string> ReadMessageAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => Console.ReadLine(), stoppingToken);
        }
    }
}