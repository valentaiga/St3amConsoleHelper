using System;

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
            if (_userLogin?.Username != username || _userLogin?.Password != password)
            {
                _userLogin = new UserLogin(username, password);
            }

            return DoLogin();
        }

        public InternalUserLogin Login(string username, string password, LoginType loginType, string verificationValue)
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

            return DoLogin();
        }

        public void InitiateLogin()
        {
            // todo: store loginResult in file/storage and read it after restart instead of new login
            var login = ReadLogin();
            var password = ReadPassword();
            var loginResult = Login(login, password);

            while (loginResult.IsError)
            {
                _logger.LogInformation($"Login failed. Reason: '{loginResult.ErrorText}'");
                if (loginResult.Result == LoginResult.BadCredentials)
                {
                    login = ReadLogin();
                    password = ReadPassword();
                    loginResult = Login(login, password);
                }

                if (loginResult.IsTwoFactorNeeded)
                {
                    Console.WriteLine($"Credentials are OK. But not signed in, reason: {loginResult.ErrorText}");
                    var steamGuardCode = ReadTwoFactor();

                    switch (loginResult.Result)
                    {
                        case LoginResult.NeedCaptcha:
                            loginResult = Login(login, password, LoginType.ByCaptcha, steamGuardCode);
                            break;
                        case LoginResult.Need2FA:
                            loginResult = Login(login, password, LoginType.ByTwoFactor, steamGuardCode);
                            break;
                        case LoginResult.NeedEmail:
                            loginResult = Login(login, password, LoginType.ByEmail, steamGuardCode);
                            break;
                    }
                }
            }

            string ReadLogin()
            {
                Console.WriteLine("Enter your login: ");
                return Console.ReadLine();
            }

            string ReadPassword()
            {
                Console.WriteLine("Enter your password: ");
                return Console.ReadLine();
            }

            string ReadTwoFactor()
            {
                Console.WriteLine("Enter your two factor code: ");
                return Console.ReadLine();
            }
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