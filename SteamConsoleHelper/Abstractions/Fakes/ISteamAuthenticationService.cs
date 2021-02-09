using System.Threading.Tasks;
using SteamConsoleHelper.Abstractions.Enums;
using SteamConsoleHelper.Abstractions.Login;

namespace SteamConsoleHelper.Abstractions.Fakes
{
    public interface ISteamAuthenticationService
    {
        LoginResult Login(string username, string password);

        LoginResult Login(string username, string password, LoginType loginType, string verificationValue);

        Task InitiateLoginAsync();
    }
}