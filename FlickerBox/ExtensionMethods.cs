using System;
using System.Globalization;

namespace FlickerBox
{
    public static class ExtensionMethods
    {
        // returns the number of milliseconds since Jan 1, 1970 (useful for converting C# dates to JS dates)
        public static string UnixTicks(this DateTime dt)
        {
            var d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dt.ToUniversalTime();
            var ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return ts.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        }
    }
}
