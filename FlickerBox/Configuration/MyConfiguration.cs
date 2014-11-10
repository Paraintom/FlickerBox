using System;
using System.Configuration;

namespace FlickerBox.Configuration
{
    public class MyConfiguration
    {
        public static bool GetBool(string myField, bool defaultValue)
        {
            bool result;
            string value = ConfigurationManager.AppSettings[myField];
            if (value == null || !bool.TryParse(value, out result))
            {
                return defaultValue;
            }
            return result;
        }
        public static string GetString(string myField, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[myField];
            return String.IsNullOrEmpty(value) ? defaultValue : value;
        }
    }
}
