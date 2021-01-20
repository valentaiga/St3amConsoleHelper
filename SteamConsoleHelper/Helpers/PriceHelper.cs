using System;

namespace SteamConsoleHelper.Helpers
{
    public static class PriceHelper
    {
        private const uint DefaultNoPriceValue = 2000;
        private const uint CheaperByValue = 3; // 0,03 rubles

        public static uint CalculateSellerPrice(uint? customerPrice, bool lowerPriceByDefault = false)
        {
            var cost = customerPrice ?? DefaultNoPriceValue;

            var sellerPrice = (uint)Math.Ceiling((double)cost * 100 / 115);
            if (lowerPriceByDefault)
            {
                sellerPrice -= CheaperByValue;
            }

            return sellerPrice;
        }
    }
}