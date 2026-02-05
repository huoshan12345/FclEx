namespace Xunit;

partial class AssertEx
{
    extension(Assert)
    {
        internal static bool HandleEqualNulls<T>([NotNullWhen(false)] T? expected, [NotNullWhen(false)] T? actual)
        {
            if (expected == null && actual == null)
                return true;

            if (expected == null || actual == null)
                throw EqualException.ForNotEqualValues(expected, actual);

            return false;
        }

        internal static bool HandleNotEqualNulls<T>([NotNullWhen(false)] T? expected, [NotNullWhen(false)] T? actual)
        {
            if (expected == null && actual == null)
                throw NotEqualException.ForEqualValues(expected, actual);

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (expected == null || actual == null)
                return true;

            return false;
        }

        public static void MembersEqual<T>(T expected, T actual, bool onlyCompareSharedMembers = false, params string[] excludedMemberPaths)
        {
            var tree = BuildExcludedMemberTree(excludedMemberPaths);
            var result = Equal(expected, actual, tree, onlyCompareSharedMembers, null, null);
            result.ThrowIfNotEqual();
        }

        public static void MembersEqual<T>(T expected, T actual, params string[] excludedMemberPaths) 
            => Assert.MembersEqual(expected, actual, false, excludedMemberPaths);

        public static void MembersNotEqual<T>(T expected, T actual, bool onlyCompareSharedMembers = false, params string[] excludedMemberPaths)
        {
            var tree = BuildExcludedMemberTree(excludedMemberPaths);
            var result = Equal(expected, actual, tree, onlyCompareSharedMembers, null, null);
            result.ThrowIfEqual();
        }

        public static void MembersNotEqual<T>(T expected, T actual, params string[] excludedMemberPaths) 
            => Assert.MembersNotEqual(expected, actual, false, excludedMemberPaths);

        public static void Equal<TEnum, TInt>(TEnum expected, TInt actual)
            where TEnum : struct, Enum
            where TInt : struct, IConvertible
        {
            Assert.Equal(expected.CastTo<TInt?>(), actual);
        }

        public static void Equal<TEnum, TInt>(TEnum? expected, TInt? actual)
            where TEnum : struct, Enum
            where TInt : struct, IConvertible
        {
            Assert.Equal(expected.CastTo<TInt?>(), actual);
        }

        public static void Equal<TEnum, TInt>(TInt? expected, TEnum actual)
            where TEnum : struct, Enum
            where TInt : struct, IConvertible
        {
            Assert.Equal(expected, actual.CastTo<TInt?>());
        }

        public static void Equal<TEnum, TInt>(TInt? expected, TEnum? actual)
            where TEnum : struct, Enum
            where TInt : struct, IConvertible
        {
            Assert.Equal(expected, actual.CastTo<TInt?>());
        }

        public static void Contains<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> collection, TKey key, TValue value)
        {
            Assert.Contains(new(key, value), collection);
        }

        public static void NotEmpty([NotNull] string? value)
        {
            Assert.NotNull(value);
            Assert.NotEmpty(value);
        }

        public static void NotEmpty([NotNull] IEnumerable? value)
        {
            Assert.NotNull(value);
            Assert.NotEmpty(value);
        }
    }
}