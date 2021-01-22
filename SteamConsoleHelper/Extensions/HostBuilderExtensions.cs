using Microsoft.Extensions.Configuration;

using SteamConsoleHelper.Abstractions.Enums;

namespace SteamConsoleHelper.Extensions
{
    public static class HostBuilderExtensions
    {
        public static bool IsFakeEnabled(this FakeService service, IConfiguration configuration)
            => configuration[$"FakeServices:{service}:IsEnabled"].ToBool();
    }
}