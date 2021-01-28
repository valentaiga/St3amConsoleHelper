using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using SteamConsoleHelper.Abstractions.Market;
using SteamConsoleHelper.Extensions;

namespace SteamConsoleHelper.Helpers
{
    public static class ParseHelper
    {
        private static readonly Regex PriceRegex = new Regex(@"([0-9])+(,)?([0-9]{0,2})");
        private static readonly Regex ListingHoverRegex = new Regex(@"mylisting_([0-9]+)_name(['0-9]+), (['0-9])+, (['0-9])+, (['0-9])+");
        private static readonly Regex ListingDescriptionRegex = new Regex(@"(This is the price the buyer pays.{3}[,0-9]+.{89}[,0-9]+.{118}[0-9a-zA-Z ]+.{70}[0-9]+.{139}[0-9]+/[0-9a-zA-Z\.\-% ]+)|(My listings awaiting confirmation)");
        private static readonly Regex NotDigitsRegex = new Regex(@"[^0-9]+");
        private static readonly Regex SpecialSymbolsRegex = new Regex(@"(\r)*(\t)*(\n)*");

        public static string KeepStringPriceOnly(this string str)
            => !string.IsNullOrEmpty(str) ? PriceRegex.Match(str).Value : null;

        public static string KeepNumbersOnly(this string str)
            => !string.IsNullOrEmpty(str) ? NotDigitsRegex.Replace(str, string.Empty) : null;

        private static string RemoveSpecialSymbols(this string str)
            => !string.IsNullOrEmpty(str) ? SpecialSymbolsRegex.Replace(str, string.Empty) : null;

        public static List<ListingHover> ParseListingHover(string str)
        {
            return ListingHoverRegex.Matches(str).Select(x =>
            {
                var parameters = x.Value.Split(',');

                var listingId = parameters[0].KeepNumbersOnly().ToULong();
                var appId = parameters[1].ToUInt();
                var contextId = parameters[2].KeepNumbersOnly().ToUInt();
                var assetId = parameters[3].KeepNumbersOnly().ToULong();

                return new ListingHover
                {
                    ListingId = listingId,
                    AppId = appId,
                    ContextId = contextId,
                    AssetId = assetId
                };
            }).ToList();
        }

        public static List<ListingDescription> ParseHtmlResult(string str)
        {
            var isAwaitingConfirmation = false;
            return ListingDescriptionRegex
                .Matches(str.RemoveSpecialSymbols()).Select(x =>
            {
                var parameters = x.Value
                    .RemoveSpecialSymbols()
                    .Split(new[] { "<br>", "class", "<id" }, StringSplitOptions.RemoveEmptyEntries);

                if (parameters.Length == 1)
                {
                    isAwaitingConfirmation = true;
                    return ListingDescription.Empty;
                }

                var buyerPrice = ParsePrice(parameters[0]).Value;
                var sellerPrice = ParsePrice(parameters[1]).Value;
                var sellDate = parameters[2]
                    .Substring(68, parameters[2].Length - parameters[2].IndexOf("</div") - 5).ToDateTime();
                var listingId = parameters[3].KeepNumbersOnly().ToULong();
                var hashName = HttpUtility.UrlDecode(parameters[5].Substring(parameters[5].LastIndexOf("/") + 1));

                if (DateTime.Today.Month == 1 && sellDate.Month == 12)
                {
                    // offset for new year prices
                    sellDate.AddYears(-1);
                }

                return new ListingDescription(
                    listingId,
                    buyerPrice,
                    sellerPrice,
                    sellDate,
                    hashName,
                    isAwaitingConfirmation);
            }).Where(x => x.ListingId != 0).ToList();
        }

        public static uint? ParsePrice(string str)
        {
            if (str == null)
            {
                return null;
            }

            var result = str
                .KeepStringPriceOnly()
                .KeepNumbersOnly()
                .ToUInt();

            if (!str.Contains(','))
            {
                result *= 100;
            }

            return result;
        }
    }
}