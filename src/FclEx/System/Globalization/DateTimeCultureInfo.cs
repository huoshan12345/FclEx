namespace System.Globalization
{
    public static class DateTimeCultureInfo
    {
        public static CultureInfo TwoDigitYear { get; } = new(CultureInfo.InvariantCulture.LCID) { Calendar = { TwoDigitYearMax = 2099 } };
    }
}