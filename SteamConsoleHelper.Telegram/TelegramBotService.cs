using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SteamConsoleHelper.Storage;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SteamConsoleHelper.Telegram
{
    public class TelegramBotService
    {
        private static ChatId AuthorChatId { get; set; }

        private readonly ILogger<TelegramBotService> _logger;
        private readonly IDataStoreService _storeService;
        private readonly TelegramBotClient _telegramBotClient;

        private string _lastMessage;

        public TelegramBotService(
            ILogger<TelegramBotService> logger, 
            IDataStoreService storeService,
            IConfiguration configuration)
        {
            _logger = logger;
            _storeService = storeService;

            try
            {
                var token = configuration["Telegram:ApiToken"];
                _telegramBotClient = new TelegramBotClient(token);
                _telegramBotClient.OnMessage += OnMessageHandler;
            }
            catch (Exception e)
            {
                logger.LogCritical($"Cant connect to telegram bot. Reason: {e.Message}");
                throw;
            }
        }

        public async Task SendMessageAsync(string text, CancellationToken stoppingToken)
        {
            if (AuthorChatId != null)
            {
                await _telegramBotClient.SendTextMessageAsync(AuthorChatId, text, cancellationToken: stoppingToken);
            }
        }

        public async Task<string> ReadMessageAsync(CancellationToken stoppingToken)
        {
            _telegramBotClient.StartReceiving(cancellationToken: stoppingToken);
            _logger.LogInformation($"Awaiting user's answer...");

            while (string.IsNullOrEmpty(_lastMessage))
            {
                await Task.Delay(TimeSpan.FromSeconds(0.1));
                if (stoppingToken.IsCancellationRequested)
                {
                    return null;
                }
            }

            _telegramBotClient.StopReceiving();
            var result = _lastMessage;
            _lastMessage = null;

            return result;
        }

        private async void OnMessageHandler(object sender, MessageEventArgs args)
        {
            var message = args.Message;

            await SaveChatIdIfNeeded(message);

            if (message.Type != MessageType.Text)
            {
                return;
            }

            try
            {
                await _telegramBotClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                _lastMessage = message.Text;
            }
            catch
            {
                // sometimes message sent earlier and already deleted
            }
        }

        private async ValueTask SaveChatIdIfNeeded(Message msg)
        {
            if (AuthorChatId != null)
            {
                return;
            }
            
            AuthorChatId = msg.Chat.Id;
            await _storeService.SaveTelegramChat(msg.Chat.Id);
        }

        public async Task InitializeAsync()
        {
            var data = await _storeService.LoadJsonBlobAsync();
            AuthorChatId = data.ChatId;
        }
    }
}