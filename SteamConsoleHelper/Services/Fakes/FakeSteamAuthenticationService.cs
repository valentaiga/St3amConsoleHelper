using Microsoft.Extensions.Configuration;

using SteamAuth;

using SteamConsoleHelper.Abstractions.Enums;
using SteamConsoleHelper.Abstractions.Fakes;
using SteamConsoleHelper.Extensions;
using SteamConsoleHelper.Resources;

using LoginResult = SteamConsoleHelper.Abstractions.Login.LoginResult;

namespace SteamConsoleHelper.Services.Fakes
{
    public class FakeSteamAuthenticationService : ISteamAuthenticationService
    {
        private readonly ProfileSettings _profileSettings;
        private readonly UserLogin _fakeUserLogin;

        public FakeSteamAuthenticationService(ProfileSettings profileSettings, IConfiguration configuration)
        {
            _profileSettings = profileSettings;

            _fakeUserLogin = new UserLogin(null, null)
            {
                SteamID = configuration["FakeServices:SteamAuthenticationService:Data:SteamID"].ToULong(),
                Session = new SessionData
                {
                    SessionID = configuration["FakeServices:SteamAuthenticationService:Data:SessionID"],
                    SteamLoginSecure = configuration["FakeServices:SteamAuthenticationService:Data:SteamLoginSecure"]
                }
            };
        }

        public LoginResult Login(string username, string password)
        {
            _profileSettings.SetUserLogin(_fakeUserLogin);
            ProfileSettings.SetIsAuthenticatedStatus(true);
            return new LoginResult(SteamAuth.LoginResult.LoginOkay);
        }

        public LoginResult Login(string username, string password, LoginType loginType, string verificationValue)
            => Login(username, password);

        public void InitiateLogin()
        {
            Login(null, null);
        }
    }
}