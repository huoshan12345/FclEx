using System;

namespace FclEx
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime _jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public const string ShortTimeFormat = "yyyyMMddHHmmss";
        public const string CnTimeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Unix时间戳
        /// 自1970年1月1日0时起的秒数
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static long ToTimestamp(this DateTime d)
        {
            return (long)(d.ToUniversalTime() - _jan1St1970).TotalSeconds;
        }

        public static long ToTimestampMilli(this DateTime d)
        {
            return (long)(d.ToUniversalTime() - _jan1St1970).TotalMilliseconds;
        }

        public static string ToShort(this DateTime @this) => @this.ToString(ShortTimeFormat);

        public static string ToCn(this DateTime @this) => @this.ToString(CnTimeFormat);

        public static DateTime AddWeek(this DateTime dateTime) => AddWeeks(dateTime, 1);

        public static DateTime AddWeeks(this DateTime dateTime, int numberOfWeeks)
        {
            return dateTime.AddDays(numberOfWeeks * 7);
        }

        public static DateTime StartOfWeek(this DateTime dt, int hour = 0, int minute = 0, int second = 0, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            var diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return Today(dt, hour, minute, second).AddDays(-1 * diff);
        }

        public static DateTime EndOfWeek(this DateTime dt, int hour = 0, int minute = 0, int second = 0, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            return StartOfWeek(dt, hour, minute, second, startOfWeek).AddDays(6);
        }

        public static DateTime Today(this DateTime dt, int hour = 0, int minute = 0, int second = 0)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, hour, minute, second);
        }

        public static DateTime Tomorrow(this DateTime dt, int hour = 0, int minute = 0, int second = 0)
        {
            return Today(dt, hour, minute, second).AddDays(1);
        }

        public static DateTime Yesterday(this DateTime dt, int hour = 0, int minute = 0, int second = 0)
        {
            return Today(dt, hour, minute, second).AddDays(-1);
        }

        public static DateTime ThisYear(this DateTime dt, int month, int day, int hour = 0, int minute = 0, int second = 0)
        {
            return new DateTime(dt.Year, month, day, hour, minute, second);
        }

        public static DateTime ThisMonth(this DateTime dt, int day, int hour = 0, int minute = 0, int second = 0)
        {
            return new DateTime(dt.Year, dt.Month, day, hour, minute, second);
        }

        public static DateTime EndOfMonth(this DateTime dt, int hour = 0, int minute = 0, int second = 0)
        {
            return new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month), hour, minute, second);
        }

        public static DateTime StartOfMonth(this DateTime dt, int hour = 0, int minute = 0, int second = 0)
        {
            return new DateTime(dt.Year, dt.Month, 1, hour, minute, second);
        }

        public static DateTime GetMaxTimeOfDate(this DateTime dt)
        {
            return dt.Date.AddDays(1).AddTicks(-1);
        }

        public static DateTime? GetMaxTimeOfDate(this DateTime? dt)
        {
            return dt?.GetMaxTimeOfDate();
        }

        public static DateTime? GetDate(this DateTime? dt)
        {
            return dt?.Date;
        }

        public static string ToStringOrEmpty(this DateTime dt, string format = CnTimeFormat)
        {
            return dt.IsDefault()
                ? string.Empty
                : dt.ToString(format);
        }

        public static string ToStringOrEmpty(this DateTime? dt, string format = CnTimeFormat)
        {
            return dt.HasValue
                ? ToStringOrEmpty(dt.Value, format)
                : string.Empty;
        }
    }
}
