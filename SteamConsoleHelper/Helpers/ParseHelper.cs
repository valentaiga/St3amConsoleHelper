using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SteamConsoleHelper.Abstractions.Market;

namespace SteamConsoleHelper.Helpers
{
    public static class ParseHelper
    {
        private static readonly Regex ListingHoverRegex = new Regex(@"mylisting_([0-9]+)_name(['0-9]+), (['0-9])+, (['0-9])+, (['0-9])+");
        private static readonly Regex NotDigitsRegex = new Regex(@"[^0-9]+");

        public static string KeepNumbersOnly(this string str)
            => NotDigitsRegex.Replace(str, string.Empty);

        public static List<ListingHover> ParseListingHover(string str)
        {
            return ListingHoverRegex.Matches(str).Select(x =>
            {
                var parameters = x.Value.Split(',');

                var listingId = Convert.ToUInt64(parameters[0].KeepNumbersOnly());
                var appId = Convert.ToUInt32(parameters[1]);
                var contextId = Convert.ToUInt32(parameters[2].KeepNumbersOnly());
                var assetId = Convert.ToUInt64(parameters[3].KeepNumbersOnly());

                return new ListingHover
                {
                    ListingId = listingId,
                    AppId = appId,
                    ContextId = contextId,
                    AssetId = assetId
                };
            }).ToList();
        }
    }
}