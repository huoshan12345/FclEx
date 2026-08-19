namespace FclEx.Extensions;

public static class CultureInfoExtensions
{
    private static readonly CultureInfo TwoDigitYear = new(CultureInfo.InvariantCulture.LCID) { Calendar = { TwoDigitYearMax = 2099 } };

    extension(CultureInfo)
    {
        public static CultureInfo TwoDigitYear => TwoDigitYear;
    }
}
