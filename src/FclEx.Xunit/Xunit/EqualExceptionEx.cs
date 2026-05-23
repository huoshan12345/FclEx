using System.Globalization;

namespace Xunit;

public static class EqualExceptionEx
{
    extension(EqualException)
    {
        public static EqualException ForNotEqualValues<T>(T? expected, T? actual, string? banner = null)
        {
            return EqualException.ForMismatchedValues(expected.ToAssertionString(), actual.ToAssertionString(), banner);
        }

        public static EqualException ForNotEqualValues<T>(T expected, T actual, TimeSpan precision, TimeSpan difference)
        {
            var actualValue = actual.ToAssertionString() +
                              (precision == TimeSpan.Zero ? "" : string.Format(CultureInfo.CurrentCulture, " (difference {0} is larger than {1})", difference, precision));

            return EqualException.ForMismatchedValues(expected.ToAssertionString(), actualValue);
        }
    }
}

public static class NotEqualExceptionEx
{
    extension(NotEqualException)
    {
        public static NotEqualException ForEqualValues<T>(T? expected, T? actual, string? banner = null)
        {
            return NotEqualException.ForEqualValues(expected.ToAssertionString(), actual.ToAssertionString(), banner);
        }

        public static NotEqualException ForEqualValues<T>(T expected, T actual, TimeSpan precision, TimeSpan difference)
        {
            var actualValue = actual.ToAssertionString() +
                              (precision == TimeSpan.Zero ? "" : string.Format(CultureInfo.CurrentCulture, " (difference {0} is not larger than {1})", difference, precision));

            return NotEqualException.ForEqualValues(expected.ToAssertionString(), actualValue);
        }
    }
}
