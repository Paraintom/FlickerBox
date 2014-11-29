using System;
using System.Globalization;

namespace FlickerBox
{
    public static class ExtensionMethods
    {
        public static string FormatWith(this string toFormat, params object[] args)
        {
            return String.Format(toFormat, args);
        }

        // returns the number of milliseconds since Jan 1, 1970 (useful for converting C# dates to JS dates)
        public static string JavascriptTicks(this DateTime dt)
        {
            var d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dt.ToUniversalTime();
            var ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return ts.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Can raise exceptions!
        /// </summary>
        /// <param name="stringJavascriptTimeStamp"></param>
        /// <returns>the corresponding unix datetime</returns>
        public static DateTime FromJavascriptTicks(this string stringJavascriptTimeStamp)
        {
            double unixTimeStamp = double.Parse(stringJavascriptTimeStamp);
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
