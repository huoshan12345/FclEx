using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Xunit.Sdk;

namespace Xunit
{
    partial class AssertExt
    {
        public static void EveryMemberEqual<T>(T expected, T actual, params string[] excludeMemberPaths)
        {
            var tree = BuildExcludeMemberTree(excludeMemberPaths);
            var result = Equal(expected, actual, tree, false);
            if (!result.equal)
                throw new EqualException(result.expected, result.actual);
        }

        public static void NotEveryMemberEqual<T>(T expected, T actual, params string[] excludeMemberPaths)
        {
            var tree = BuildExcludeMemberTree(excludeMemberPaths);
            var result = Equal(expected, actual, tree, false);
            if (result.equal)
                throw new NotEqualException(result.expected?.ToString(), result.actual?.ToString());
        }

        public static void EverySameNameMemberEqual<T1, T2>(T1 expected, T2 actual, params string[] excludeMemberPaths)
        {
            var tree = BuildExcludeMemberTree(excludeMemberPaths);
            var result = Equal(expected, actual, tree, true);
            if (!result.equal)
                throw new EqualException(result.expected, result.actual);
        }

        public static void NotEverySameNameMemberEqual<T1, T2>(T1 expected, T2 actual, params string[] excludeMemberPaths)
        {
            var tree = BuildExcludeMemberTree(excludeMemberPaths);
            var result = Equal(expected, actual, tree, true);
            if (result.equal)
                throw new NotEqualException(result.expected?.ToString(), result.actual?.ToString());
        }

        /// <summary>
        /// Verifies that two <see cref="T:System.DateTime" /> values are equal, within the precision
        /// given by <paramref name="precision" />.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="precision">The allowed difference in time where the two dates are considered equal</param>
        /// <exception cref="T:Xunit.Sdk.EqualException">Thrown when the values are not equal</exception>
        public static void Equal(TimeSpan expected, TimeSpan actual, TimeSpan precision)
        {
            var timeSpan = (expected - actual).Duration();
            if (timeSpan > precision)
            {
                throw new EqualException(
                    expected: $"{expected} ",
                    actual: $"{actual} difference {timeSpan} is larger than {precision}");
            }
        }

        public static void NotEmpty([NotNull] string? value)
        {
            Assert.NotNull(value);
            Assert.NotEmpty(value);
        }

    }
}
