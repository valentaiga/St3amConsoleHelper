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

        public async Task InitializeLoginAsync()
        {
            // todo: store loginResult in file/storage and read it after restart instead of new login
            var login = await ReadLoginAsync();
            var password = await ReadPasswordAsync();
            var loginResult = await LoginAsync(login, password);

            while (loginResult.IsError)
            {
                _logger.LogInformation($"Login failed. Reason: '{loginResult.ErrorText}'");
                if (loginResult.Result == LoginResult.BadCredentials || loginResult.Result == LoginResult.GeneralFailure)
                {
                    login = await ReadLoginAsync();
                    password = await ReadPasswordAsync();
                    loginResult = await LoginAsync(login, password);
                }

                if (loginResult.IsTwoFactorNeeded)
                {
                    await _messageProvider.SendMessageAsync($"Credentials are OK. But not signed in, reason: {loginResult.ErrorText}");
                    var steamGuardCode = await ReadTwoFactorAsync();

                    switch (loginResult.Result)
                    {
                        case LoginResult.NeedCaptcha:
                            loginResult = await LoginAsync(login, password, LoginType.ByCaptcha, steamGuardCode);
                            break;
                        case LoginResult.Need2FA:
                            loginResult = await LoginAsync(login, password, LoginType.ByTwoFactor, steamGuardCode);
                            break;
                        case LoginResult.NeedEmail:
                            loginResult = await LoginAsync(login, password, LoginType.ByEmail, steamGuardCode);
                            break;
                    }
                }
            }

            await _messageProvider.SendMessageAsync("Successfully signed in");
        }

        private async Task<string> ReadLoginAsync()
        {
            if (!string.IsNullOrEmpty(Settings.UserLogin?.Username))
            {
                return Settings.UserLogin.Username;
            }

            await _messageProvider.SendMessageAsync("Enter your login:");
            return await _messageProvider.ReadMessageAsync();
        }

        private async ValueTask<string> ReadPasswordAsync()
        {
            if (!string.IsNullOrEmpty(Settings.UserLogin?.Password))
            {
                return Settings.UserLogin.Password;
            }

            await _messageProvider.SendMessageAsync("Enter your password:");
            return await _messageProvider.ReadMessageAsync();
        }

        private async Task<string> ReadTwoFactorAsync()
        {
            await _messageProvider.SendMessageAsync("Enter your two factor code:");
            return await _messageProvider.ReadMessageAsync();
        }

        private async ValueTask<InternalUserLogin> LoginAsync(string username, string password)
        {
            if (_userLogin?.Username != username || _userLogin?.Password != password)
            {
                _userLogin = new UserLogin(username, password);
            }

            return await DoLoginAsync();
        }

        private async ValueTask<InternalUserLogin> LoginAsync(string username, string password, LoginType loginType, string verificationValue)
        {
            if (_userLogin?.Username != username || _userLogin?.Password != password)
            {
                _userLogin = new UserLogin(username, password);
            }

            switch (loginType)
            {
                case LoginType.ByEmail:
                    _userLogin.EmailCode = verificationValue;
                    break;
                case LoginType.ByTwoFactor:
                    _userLogin.TwoFactorCode = verificationValue;
                    break;
                case LoginType.ByCaptcha:
                    _userLogin.CaptchaText = verificationValue;
                    break;
            }

            return await DoLoginAsync();
        }

        private async ValueTask<InternalUserLogin> DoLoginAsync()
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
                        await _messageProvider.SendMessageAsync(message);
                        return new InternalUserLogin(result, message);
                    }

                case LoginResult.Need2FA:
                    {
                        var message = "Need to confirm mobile authenticator code text in telegram";
                        _logger.LogWarning(message);
                        await _messageProvider.SendMessageAsync(message);
                        return new InternalUserLogin(result, message);
                    }

                case LoginResult.BadCredentials:
                case LoginResult.GeneralFailure:
                    return new InternalUserLogin(result, "Bad credentials");

                case LoginResult.TooManyFailedLogins:
                    return new InternalUserLogin(result, "Too many failed logins");

                case LoginResult.LoginOkay:
                    {
                        ClearTwoFactorCode();
                        Settings.SetIsAuthenticatedStatus(true);
                        Settings.SetUserLogin(_userLogin);
                        await _storeService.SaveCredentialsAsync(_userLogin);
                        return new InternalUserLogin(LoginResult.LoginOkay);
                    }
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

            var data = await _storeService.LoadJsonBlobAsync();
            if (data.UserLogin.Username != null)
            {
                Settings.SetUserLogin(data.UserLogin);
                Settings.SetIsAuthenticatedStatus(true);
            }
            else
            {
                await InitializeLoginAsync();
            }
        }
    }
}