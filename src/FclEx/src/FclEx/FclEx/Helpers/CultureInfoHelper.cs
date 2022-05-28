using System.Globalization;

namespace FclEx.Helpers;

public static class CultureInfoHelper
{
    public static CultureInfo TwoDigitYear { get; } = new(CultureInfo.InvariantCulture.LCID) { Calendar = { TwoDigitYearMax = 2099 } };
}