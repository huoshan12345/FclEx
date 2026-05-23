namespace FclEx.Helpers;

public class MethodTests
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
    public void GetMethod_Action_Test()
    {
        var actual = Method.Of(Tester.Action);
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
    public void GetMethod_Action_T_Test()
    {
        var actual = Method.Of<int>(Tester.Action);
        var expected = GetMethod(nameof(Tester.Action), 1);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetMethod_Action_T2_Test()
    {
        var actual = Method.Of<int, int>(Tester.Action);
        var expected = GetMethod(nameof(Tester.Action), 2);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetMethod_Func_Test()
    {
        var actual = Method.Of(Tester.Func);
        var expected = GetMethod(nameof(Tester.Func), 0);
        Assert.Equal(expected, actual);
    }


    [Fact]
    public void GetMethod_Func_T_Test()
    {
        var actual = Method.Of<int, int>(Tester.Func);
        var expected = GetMethod(nameof(Tester.Func), 1);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetMethod_Func_T2_Test()
    {
        var actual = Method.Of<int, int, int>(Tester.Func);
        var expected = GetMethod(nameof(Tester.Func), 2);
        Assert.Equal(expected, actual);
    }
}