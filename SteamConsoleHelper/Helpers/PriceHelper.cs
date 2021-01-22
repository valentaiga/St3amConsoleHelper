using System;

namespace SteamConsoleHelper.Helpers
{
    public static class PriceHelper
    {
        public const uint ExpensivePrice = 100_00;

        private const uint DefaultNoPriceValue = 2000;
        private const uint CheaperByValue = 3; // 0,03 rubles
        private const uint CheaperByBigValue = 5_00;

        public static uint CalculateSellerPrice(uint? customerPrice, bool lowerPriceByDefault = false)
        {
            var cost = customerPrice ?? DefaultNoPriceValue;

            var sellerPrice = (uint)Math.Ceiling((double)cost * 100 / 115);
            if (lowerPriceByDefault)
            {
                sellerPrice = sellerPrice > ExpensivePrice
                    ? sellerPrice - CheaperByValue
                    : sellerPrice - CheaperByBigValue;
            }

            return sellerPrice;
        }
    }
}