namespace FclEx.Extensions.EnumExtensions;

public class ToIntTests
{
    private enum TesterOfInt : int
    {
        No = 0,
        Yes = 1,
        Max = int.MaxValue,
    }

    private enum TesterOfShort : short
    {
        No = 0,
        Yes = 1,
        Max = short.MaxValue,
    }

    private enum TesterOfByte : byte
    {
        No = 0,
        Yes = 1,
        Max = byte.MaxValue,
    }

    private enum TesterOfLong : long
    {
        No = 0,
        Yes = 1,
        Max = long.MaxValue,
    }

    private static void ToIntTest<T>() where T : struct, Enum, IConvertible
    {
        var values = EnumHelper.GetValues<T>();
        foreach (var value in values)
        {
            var expected = value.ToInt32(null);
            Assert.Equal(expected, value.ToInt());
        }
    }

    private static void ToLongTest<T>() where T : struct, Enum, IConvertible
    {
        var values = EnumHelper.GetValues<T>();
        foreach (var value in values)
        {
            var expected = value.ToInt64(null);
            Assert.Equal(expected, value.ToLong());
        }
    }

    private static readonly MethodInfo[] _methods =
        typeof(ToIntTests).GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

    private static readonly MethodInfo _methodOfToIntTest = _methods.First(m => m.Name == nameof(ToIntTest));

    private static readonly MethodInfo _methodOfToLongTest = _methods.First(m => m.Name == nameof(ToLongTest));

    [Fact]
    public void ToInt_Test()
    {
        var types = new[] { typeof(TesterOfInt), typeof(TesterOfShort), typeof(TesterOfByte), };
        foreach (var type in types)
        {
            var method = _methodOfToIntTest.MakeGenericMethod(type);
            method.Invoke(null, null);
        }
    }

    [Fact]
    public void ToLong_Test()
    {
        var types = new[] { typeof(TesterOfInt), typeof(TesterOfShort), typeof(TesterOfByte), typeof(TesterOfLong) };
        foreach (var type in types)
        {
            var method = _methodOfToLongTest.MakeGenericMethod(type);
            method.Invoke(null, null);
        }
    }
}