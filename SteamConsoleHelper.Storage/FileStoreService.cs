using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using SteamAuth;

using SteamConsoleHelper.Storage.Models;

namespace SteamConsoleHelper.Storage
{
    public class FileStoreService : IDataStoreService
    {
        private const string DataBlobFileName = "DataBlob.json";
        
        private static readonly Semaphore Locker = new Semaphore(1, 1);

        private readonly ILogger<FileStoreService> _logger;

        public FileStoreService(ILogger<FileStoreService> logger)
        {
            _logger = logger;
        }

        public async Task SaveCredentialsAsync(UserLogin userLogin)
        {
            Locker.WaitOne();
            try
            {
                var dataBlob = await LoadJsonBlobAsync();
                dataBlob.UserLogin = userLogin;
                await SaveJsonBlobAsync(dataBlob);
            }
            finally
            {
                Locker.Release(1);
            }
        }

        public async Task SaveTelegramChat(long chatId)
        {
            Locker.WaitOne();
            try
            {
                var dataBlob = await LoadJsonBlobAsync();
                dataBlob.ChatId = chatId;
                await SaveJsonBlobAsync(dataBlob);
            }
            finally
            {
                Locker.Release(1);
            }
        }

        public async Task<DataBlob> LoadJsonBlobAsync()
        {
            try
            {
                await using var fs = File.Open(DataBlobFileName, FileMode.OpenOrCreate);
                using var reader = new StreamReader(fs);

                var json = await reader.ReadToEndAsync();
                _logger.LogDebug($"Successfully loaded data json with {json.Length} chars in it");

                return string.IsNullOrEmpty(json)
                    ? new DataBlob()
                    : JsonConvert.DeserializeObject<DataBlob>(json);
            }
            catch
            {
                _logger.LogError("Failed to read blob from disk. Returned empty blob.");
                return new DataBlob();
            }
        }

        private async Task SaveJsonBlobAsync(DataBlob dataBlob)
        {
            await using var fs = File.Open(DataBlobFileName, FileMode.Create);
            await using var writer = new StreamWriter(fs);
            var json = JsonConvert.SerializeObject(dataBlob);
            await writer.WriteAsync(json);
        }
    }
}