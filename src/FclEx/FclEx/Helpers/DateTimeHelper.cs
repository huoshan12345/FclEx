using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly DateTime _jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime TimestampToDateTime(long timestamp)
            => _jan1St1970.AddSeconds(timestamp);

        public static DateTime TimestampMilliToDateTime(long timestampMilli)
            => TimestampToDateTime((long)Math.Round(timestampMilli / 1000d));
    }
}
