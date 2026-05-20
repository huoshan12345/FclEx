namespace Xunit;

partial class AssertEx
{
    extension(Assert)
    {
        public static void Equal(DateTime? expected, DateTime? actual, TimeSpan precision)
        {
            if (Assert.HandleEqualNulls(expected, actual))
                return;

            Assert.Equal(expected.Value, actual.Value, precision);
        }

        public static void NotEqual(DateTime? expected, DateTime? actual, TimeSpan precision)
        {
            if (Assert.HandleNotEqualNulls(expected, actual))
                return;

            var (e, a) = (expected.Value, actual.Value);
            var difference = (e - a).Duration();
            if (difference >= precision)
                return;

            Assert.NotEqual(e, a, new PrecisionDateTimeOffsetComparer(precision));
        }

        public static void Equal(TimeSpan? expected, TimeSpan? actual, TimeSpan precision)
        {
            if (Assert.HandleEqualNulls(expected, actual))
                return;

            var (e, a) = (expected.Value, actual.Value);
            var difference = (e - a).Duration();
            if (difference <= precision)
                return;

            throw EqualException.ForNotEqualValues(e, a, precision, difference);
        }

        public static void NotEqual(TimeSpan? expected, TimeSpan? actual, TimeSpan precision)
        {
            if (Assert.HandleNotEqualNulls(expected, actual))
                return;

            var (e, a) = (expected.Value, actual.Value);
            var difference = (e - a).Duration();
            if (difference >= precision)
                return;

            throw NotEqualException.ForEqualValues(e, a, precision, difference);
        }

        public static void Equal(DateTimeOffset? expected, DateTimeOffset? actual, TimeSpan precision)
        {
            if (Assert.HandleEqualNulls(expected, actual))
                return;

            Assert.Equal(expected.Value, actual.Value, precision);
        }

        public static void NotEqual(DateTimeOffset? expected, DateTimeOffset? actual, TimeSpan precision)
        {
            if (Assert.HandleNotEqualNulls(expected, actual))
                return;

            var (e, a) = (expected.Value, actual.Value);
            var difference = (e - a).Duration();
            if (difference >= precision)
                return;

            throw NotEqualException.ForEqualValues(e, a, precision, difference);
        }

        /// <summary>
        /// Asserts that two <see cref="DateTimeOffset"/> values are equal when compared
        /// at millisecond precision.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <remarks>
        /// Sub-millisecond precision (ticks) is ignored during the comparison.
        /// This is useful when working with systems or databases that do not support
        /// microsecond or tick-level precision.
        /// </remarks>
        public static void EqualToMilliseconds(DateTimeOffset? expected, DateTimeOffset? actual)
        {
            if (Assert.HandleEqualNulls(expected, actual))
                return;

            Assert.Equal(expected.Value.TruncateToMilliseconds(), actual.Value.TruncateToMilliseconds());
        }

        /// <summary>
        /// Asserts that two <see cref="DateTimeOffset"/> values are equal when compared
        /// at second precision.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <remarks>
        /// Sub-second precision (milliseconds and ticks) is ignored during the comparison.
        /// This is useful when working with systems or databases that do not support
        /// microsecond or tick-level precision.
        /// </remarks>
        public static void EqualToSeconds(DateTimeOffset? expected, DateTimeOffset? actual)
        {
            if (Assert.HandleEqualNulls(expected, actual))
                return;

            Assert.Equal(expected.Value.TruncateToSeconds(), actual.Value.TruncateToSeconds());
        }

        /// <summary>
        /// Asserts that two <see cref="DateTimeOffset"/> values are not equal when compared
        /// at millisecond precision.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <remarks>
        /// Sub-millisecond precision (ticks) is ignored during the comparison.
        /// This is useful when working with systems or databases that do not support
        /// microsecond or tick-level precision.
        /// </remarks>
        public static void NotEqualToMilliseconds(DateTimeOffset? expected, DateTimeOffset? actual)
        {
            if (Assert.HandleNotEqualNulls(expected, actual))
                return;

            Assert.NotEqual(expected.Value.TruncateToMilliseconds(), actual.Value.TruncateToMilliseconds());
        }

        /// <summary>
        /// Asserts that two <see cref="DateTimeOffset"/> values are not equal when compared
        /// at second precision.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <remarks>
        /// Sub-second precision (milliseconds and ticks) is ignored during the comparison.
        /// This is useful when working with systems or databases that do not support
        /// microsecond or tick-level precision.
        /// </remarks>
        public static void NotEqualToSeconds(DateTimeOffset? expected, DateTimeOffset? actual)
        {
            if (Assert.HandleNotEqualNulls(expected, actual))
                return;

            Assert.NotEqual(expected.Value.TruncateToSeconds(), actual.Value.TruncateToSeconds());
        }
    }
}
