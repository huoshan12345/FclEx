using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FclEx
{
    public static class StringParseExtensions
    {
        public static int ToInt(this string str,
            NumberStyles style = NumberStyles.Integer,
            IFormatProvider? provider = null,
            int defaultValue = default)
        {
            return int.TryParse(str, style, provider, out var r) ? r : defaultValue;
        }

        public static bool ToBool(this string str, bool defaultValue = default)
        {
            return bool.TryParse(str, out var r) ? r : defaultValue;
        }

        public static double ToDouble(this string str,
            NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands,
            IFormatProvider? provider = null,
            double defaultValue = default)
        {
            return double.TryParse(str, style, provider, out var r) ? r : defaultValue;
        }

        public static DateTime ToDateTime(this string str, 
            IFormatProvider? provider = null, 
            DateTimeStyles styles = DateTimeStyles.None,
            DateTime defaultValue = default)
        {
            return DateTime.TryParse(str, provider, styles, out var r) ? r : defaultValue;
        }
    }
}
