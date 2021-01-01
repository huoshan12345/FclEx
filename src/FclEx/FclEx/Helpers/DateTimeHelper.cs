using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly DateTime _jan1St1970 = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromUnixTimeSeconds(long timestamp)
            => _jan1St1970.AddSeconds(timestamp);

        public static DateTime FromUnixTimeMilli(long timestampMilli)
            => FromUnixTimeSeconds((long)Math.Round(timestampMilli / 1000d));
    }
}
