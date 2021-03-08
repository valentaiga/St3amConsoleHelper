using System;
using System.Threading.Tasks;

namespace SteamConsoleHelper.Services.Messenger
{
    public class ConsoleProvider : IMessageProvider
    {
        public Task SendMessageAsync(string message)
        {
            return Task.Run(() => Console.WriteLine(message));
        }

        public Task<string> ReadMessageAsync()
        {
            return Task.Run(() => Console.ReadLine());
        }
    }
}