using Microsoft.Extensions.Logging;

using SteamAuth;

using SteamConsoleHelper.Abstractions.Enums;
using SteamConsoleHelper.Abstractions.Fakes;
using SteamConsoleHelper.Resources;

using InternalUserLogin = SteamConsoleHelper.Abstractions.Login.LoginResult;

namespace SteamConsoleHelper.Services
{
    public class SteamAuthenticationService : ISteamAuthenticationService
    {
        private readonly ILogger<SteamAuthenticationService> _logger;
        private readonly ProfileSettings _profileSettings;

        private UserLogin _userLogin;

        public SteamAuthenticationService(ILogger<SteamAuthenticationService> logger, ProfileSettings profileSettings)
        {
            _logger = logger;
            _profileSettings = profileSettings;
        }

        public InternalUserLogin Login(string username, string password)
        {
            // todo: request twoFactorCode/emailCode/captcha in tg
            // todo: start bot using tg command: like '/start <login> <password> <twoFactorCode>'
            if (_userLogin.Username != username || _userLogin.Password != password)
            {
                _userLogin = new UserLogin(username, password);
            }

            return DoLogin();
        }

        public InternalUserLogin Login(string username, string password, LoginType loginType, string verificationValue)
        {
            if (_userLogin.Username != username || _userLogin.Password != password)
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

            return DoLogin();
        }

        private InternalUserLogin DoLogin()
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
                        return new InternalUserLogin(result, message);
                    }

                case LoginResult.Need2FA:
                    {
                        var message = "Need to confirm mobile authenticator code text in telegram";
                        _logger.LogWarning(message);
                        return new InternalUserLogin(result, message);
                    }

                case LoginResult.BadCredentials:
                    return new InternalUserLogin(result, "Bad credentials");

                case LoginResult.TooManyFailedLogins:
                    return new InternalUserLogin(result, "Too many failed logins");

                case LoginResult.LoginOkay:
                {
                    _profileSettings.SetUserLogin(_userLogin);
                    return new InternalUserLogin(LoginResult.LoginOkay);
                }
                default:
                    return new InternalUserLogin(result, "Unexpected login failure");
            }
        }
    }
}