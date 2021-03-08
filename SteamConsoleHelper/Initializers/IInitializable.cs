using System.Threading.Tasks;

namespace SteamConsoleHelper.Initializers
{
    public interface IInitializable
    {
        public ValueTask InitializeAsync();
    }
}