using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using SteamAuth;

using SteamConsoleHelper.Abstractions.Fakes;
using SteamConsoleHelper.Extensions;
using SteamConsoleHelper.Resources;

using LoginResult = SteamConsoleHelper.Abstractions.Login.LoginResult;

namespace SteamConsoleHelper.Services.Fakes
{
    public class FakeSteamAuthenticationService : ISteamAuthenticationService
    {
        private readonly UserLogin _fakeUserLogin;

        public FakeSteamAuthenticationService(IConfiguration configuration)
        {
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

        private LoginResult FakeLogin()
        {
            Settings.SetUserLogin(_fakeUserLogin);
            Settings.SetIsAuthenticatedStatus(true);
            return new LoginResult(SteamAuth.LoginResult.LoginOkay);
        }

        public Task InitiateLoginAsync()
        {
            FakeLogin();
            return Task.CompletedTask;
        }
    }
}