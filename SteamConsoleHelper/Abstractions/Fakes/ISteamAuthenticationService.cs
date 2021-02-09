using System.Threading.Tasks;

namespace SteamConsoleHelper.Abstractions.Fakes
{
    public interface ISteamAuthenticationService
    {
        Task InitiateLoginAsync();
    }
}