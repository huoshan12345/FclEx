using System.Linq;
using Xunit;

namespace System.Reflection
{
    public class MethodHelperTests
    {
        internal static class Tester
        {
            public static void Action() { }
            public static void Action(int param) { }
            public static void Action(int param, int param2) { }

            public static int Func() => 0;
            public static int Func(int param) => 0;
            public static int Func(int param, int param2) => 0;
        }

        [Fact]
        public void GetMethodInfo_Action_Test()
        {
            var actual = MethodHelper.GetMethodInfo(Tester.Action);
            var expected = GetMethod(nameof(Tester.Action), 0);
            Assert.Equal(expected, actual);
        }

        private static MethodInfo GetMethod(string name, int paramCount)
        {
            // ReSharper disable once ReplaceWithSingleCallToSingle
            return typeof(Tester).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == name && m.GetParameters().Length == paramCount)
                .Single();
        }

        [Fact]
        public void GetMethodInfo_Action_T_Test()
        {
            var actual = MethodHelper.GetMethodInfo<int>(Tester.Action);
            var expected = GetMethod(nameof(Tester.Action), 1);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetMethodInfo_Action_T2_Test()
        {
            var actual = MethodHelper.GetMethodInfo<int, int>(Tester.Action);
            var expected = GetMethod(nameof(Tester.Action), 2);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetMethodInfo_Func_Test()
        {
            var actual = MethodHelper.GetMethodInfo(Tester.Func);
            var expected = GetMethod(nameof(Tester.Func), 0);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void GetMethodInfo_Func_T_Test()
        {
            var actual = MethodHelper.GetMethodInfo<int, int>(Tester.Func);
            var expected = GetMethod(nameof(Tester.Func), 1);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetMethodInfo_Func_T2_Test()
        {
            var actual = MethodHelper.GetMethodInfo<int, int, int>(Tester.Func);
            var expected = GetMethod(nameof(Tester.Func), 2);
            Assert.Equal(expected, actual);
        }
    }
}
