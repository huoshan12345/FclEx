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
    }
}
