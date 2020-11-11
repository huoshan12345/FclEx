using System.Globalization;

namespace FclEx.Helpers
{
    public static class DateTimeCultureInfo
    {
        public static CultureInfo TwoDigitYear { get; }


        static DateTimeCultureInfo()
        {
            var ci = new CultureInfo(CultureInfo.InvariantCulture.LCID);
            ci.Calendar.TwoDigitYearMax = 2099;
            TwoDigitYear = ci;
        }
    }
}