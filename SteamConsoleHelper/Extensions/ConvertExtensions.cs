using System;

namespace SteamConsoleHelper.Extensions
{
    public static class ConvertExtensions
    {
        public static bool ToBool(this string str)
            => bool.Parse(str);

        public static uint ToUInt(this string str)
            => Convert.ToUInt32(str);

        public static ulong ToULong(this string str)
            => Convert.ToUInt64(str);

        public static DateTime ToDateTime(this string str)
            => DateTime.Parse(str);
    }
}