﻿using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SteamConsoleHelper.Extensions
{
    public static class StringExtensions
    {
        public static string[] Split(this string str, string[] separators)
        {
            var result = new List<string>();

            foreach (var separator in separators)
            {
                if (str.Contains(separator))
                {
                    var array = str.Split(separator);
                    result.AddRange(array.SelectMany(x => x.Split(separators)));
                }
            }

            return result.ToArray();
        }

        public static string ToEncodedUrl(this string str)
            => WebUtility.UrlEncode(str).Replace("+", "%20");
    }
}