using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SteamAuth;

using SteamConsoleHelper.Abstractions.Enums;
using SteamConsoleHelper.Resources;
using SteamConsoleHelper.Services.Messenger;
using SteamConsoleHelper.Storage;

using InternalUserLogin = SteamConsoleHelper.Abstractions.Login.LoginResult;

namespace SteamConsoleHelper.Services
{
    public class SteamAuthenticationService
    {
        private readonly IDataStoreService _storeService;
        private readonly ILogger<SteamAuthenticationService> _logger;
        private readonly IMessageProvider _messageProvider;

        private static UserLogin _userLogin;

        public SteamAuthenticationService(
            IDataStoreService storeService,
            ILogger<SteamAuthenticationService> logger,
            IMessageProvider messageProvider)
        {
            _storeService = storeService;
            _logger = logger;
            _messageProvider = messageProvider;
        }

        public async Task InitializeLoginAsync(CancellationToken stoppingToken)
        {
            var login = await ReadLoginAsync(stoppingToken);
            var password = await ReadPasswordAsync(stoppingToken);
            var loginResult = await LoginAsync(login, password, stoppingToken);

            while (loginResult.IsError)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                switch (loginResult.Result)
                {
                    case LoginResult.LoginOkay:
                        break;
                    case LoginResult.TooManyFailedLogins:
                    case LoginResult.GeneralFailure:
                    {
                        _logger.LogInformation($"Login failed. Reason: '{loginResult.ErrorText}'");
                        return;
                    }
                    case LoginResult.BadRSA:
                    {
                        _logger.LogCritical("Steam authorization servers are down. Delayed for 30 secs");
                        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                        break;
                    }
                    case LoginResult.BadCredentials:
                    {
                        login = await ReadLoginAsync(stoppingToken);
                        password = await ReadPasswordAsync(stoppingToken);
                        loginResult = await LoginAsync(login, password, stoppingToken);
                        break;
                    }
                    case LoginResult.NeedCaptcha:
                    case LoginResult.Need2FA:
                    case LoginResult.NeedEmail:
                    {
                        await _messageProvider.SendMessageAsync($"Credentials are OK. But not signed in, reason: {loginResult.ErrorText}", stoppingToken);
                        var steamGuardCode = await ReadTwoFactorAsync(stoppingToken);

                        loginResult = loginResult.Result switch
                        {
                            LoginResult.NeedCaptcha => await LoginAsync(login, password, LoginType.ByCaptcha, steamGuardCode, stoppingToken),
                            LoginResult.Need2FA => await LoginAsync(login, password, LoginType.ByTwoFactor, steamGuardCode, stoppingToken),
                            LoginResult.NeedEmail => await LoginAsync(login, password, LoginType.ByEmail, steamGuardCode, stoppingToken),
                            _ => loginResult
                        };
                        break;
                    }
                }
            }

            await _messageProvider.SendMessageAsync("Successfully signed in", stoppingToken);
        }

        private async Task<string> ReadLoginAsync(CancellationToken stoppingToken)
        {
            if (!string.IsNullOrEmpty(Settings.UserLogin?.Username))
            {
                return Settings.UserLogin.Username;
            }

            await _messageProvider.SendMessageAsync("Enter your login:", stoppingToken);
            return await _messageProvider.ReadMessageAsync(stoppingToken);
        }

        private async ValueTask<string> ReadPasswordAsync(CancellationToken stoppingToken)
        {
            if (!string.IsNullOrEmpty(Settings.UserLogin?.Password))
            {
                return Settings.UserLogin.Password;
            }

            await _messageProvider.SendMessageAsync("Enter your password:", stoppingToken);
            return await _messageProvider.ReadMessageAsync(stoppingToken);
        }

        private async Task<string> ReadTwoFactorAsync(CancellationToken stoppingToken)
        {
            await _messageProvider.SendMessageAsync("Enter your two factor code:", stoppingToken);
            return await _messageProvider.ReadMessageAsync(stoppingToken);
        }

        private async ValueTask<InternalUserLogin> LoginAsync(string username, string password, CancellationToken stoppingToken)
        {
            if (_userLogin?.Username != username || _userLogin?.Password != password)
            {
                _userLogin = new UserLogin(username, password);
            }

            return await DoLoginAsync(stoppingToken);
        }

        private async ValueTask<InternalUserLogin> LoginAsync(string username, string password, LoginType loginType, string verificationValue, CancellationToken stoppingToken)
        {
            if (_userLogin?.Username != username || _userLogin?.Password != password)
            {
                _userLogin = new UserLogin(username, password);
            }

            switch (loginType)
            {
                case LoginType.ByEmail:
                {
                    _userLogin!.EmailCode = verificationValue;
                    break;
                }
                case LoginType.ByTwoFactor:
                {
                    _userLogin!.TwoFactorCode = verificationValue;
                    break;
                }
                case LoginType.ByCaptcha:
                {
                    _userLogin!.CaptchaText = verificationValue;
                    break;
                }
            }

            return await DoLoginAsync(stoppingToken);
        }

        private async ValueTask<InternalUserLogin> DoLoginAsync(CancellationToken stoppingToken)
        {
            var result = _userLogin.DoLogin();

            switch (result)
            {
                case LoginResult.NeedEmail:
                    {
                        var message = "Need to confirm email code in telegram";
                        _logger.LogWarning(message);
                        return new InternalUserLogin(result, message);
                    }

                case LoginResult.NeedCaptcha:
                    {
                        // need to open link and get text from captcha
                        var captchaUrl = APIEndpoints.COMMUNITY_BASE + "/public/captcha.php?gid=" +
                                         _userLogin.CaptchaGID;
                        var message = $"Need to confirm captcha text in telegram : {captchaUrl}";
                        _logger.LogWarning(message);
                        await _messageProvider.SendMessageAsync(message, stoppingToken);
                        return new InternalUserLogin(result, message);
                    }

                case LoginResult.Need2FA:
                    {
                        var message = "Need to confirm mobile authenticator code text in telegram";
                        _logger.LogWarning(message);
                        await _messageProvider.SendMessageAsync(message, stoppingToken);
                        return new InternalUserLogin(result, message);
                    }

                case LoginResult.BadCredentials:
                    return new InternalUserLogin(result, "Bad credentials");

                case LoginResult.LoginOkay:
                    {
                        ClearTwoFactorCode();
                        Settings.SetIsAuthenticatedStatus(true);
                        Settings.SetUserLogin(_userLogin);
                        await _storeService.SaveCredentialsAsync(_userLogin);
                        return new InternalUserLogin(LoginResult.LoginOkay);
                    }
                case LoginResult.GeneralFailure:
                case LoginResult.BadRSA:
                case LoginResult.TooManyFailedLogins:
                default:
                    return new InternalUserLogin(result, "Unexpected login failure");
            }

            static void ClearTwoFactorCode()
            {
                // prevent using expired second factor
                _userLogin.EmailCode = _userLogin.TwoFactorCode = _userLogin.CaptchaText = null;
            }
        }

        public async ValueTask InitializeAsync()
        {
            if (Settings.IsAuthenticated)
            {
                return;
            }

            var credentials = await _storeService.GetCredentialsAsync();
            if (credentials != null)
            {
                Settings.SetUserLogin(credentials);
                Settings.SetIsAuthenticatedStatus(true);
            }
            else
            {
                await InitializeLoginAsync(new CancellationToken());
            }
        }
    }
}