using FclEx.TestModels;

namespace FclEx.Helpers;

public class UnsafeHelperTests(ITestOutputHelper output)
{
    public static readonly IEnumerable<object[]> BuiltInValueTypeCases = Types.CommonValueTypes.Select(m => new object[] { m });

    private static readonly MethodInfo _sizeOfTTest = typeof(UnsafeHelperTests).GetRequiredMethod(nameof(SizeOf_T_Test));
    private static readonly MethodInfo _sizeOf = typeof(UnsafeHelper).GetRequiredMethod(nameof(UnsafeHelper.SizeOf));
    private static readonly MethodInfo _unsafeSizeOf = typeof(Unsafe).GetRequiredMethod(nameof(Unsafe.SizeOf));

    [Theory]
    [MemberData(nameof(BuiltInValueTypeCases))]
    public void SizeOf_BuiltInValueType_Test(Type type)
    {
        var size = _sizeOfTTest.MakeGenericMethod(type).Invoke<int>(null, null);
        output.WriteLine($"{type.ShortName()}'s size: " + size);
    }

    [Fact]
    public void SizeOf_Struct_Test()
    {
        Assert.Equal(Unsafe.SizeOf<TestStruct>(), UnsafeHelper.SizeOf<TestStruct>());
    }

    [Theory]
    [InlineData(typeof(object))]
    [InlineData(typeof(string))]
    [InlineData(typeof(List<string>))]
    [InlineData(typeof(List<int>))]
    public unsafe void SizeOf_ReferenceType_Test(Type type)
    {
        var size = SizeOf(type);
        Assert.Equal(sizeof(IntPtr), size); // 8 for 64-bit
    }

    [Theory]
    [InlineData(typeof(List<>))]
    [InlineData(typeof(Dictionary<,>))]
    public void SizeOf_OpenGeneric_Test(Type type)
    {
        Assert.Throws<InvalidOperationException>(() => SizeOf(type));
    }

    private static unsafe int SizeOf_T_Test<T>()
    {
        var size = sizeof(T);
        Assert.Equal(size, UnsafeHelper.SizeOf<T>());
        return size;
    }

    private static int SizeOf(Type type)
    {
        return _sizeOf.MakeGenericMethod(type).Invoke<int>(null, null);
    }

    private static int UnsafeSizeOf(Type type)
    {
        return _unsafeSizeOf.MakeGenericMethod(type).Invoke<int>(null, null);
    }

    [Fact]
    public void CompareSizeOf()
    {
        var table = new ConsoleTable(new()
        {
            Columns = ["Type", nameof(Marshal), nameof(Unsafe), nameof(SizeCalculator), nameof(UnsafeHelper)],
            RenderColumns = true,
        });

        var types = Types.CommonValueTypes.Concat([
            typeof(TestStruct),
            typeof(TestRecord),
            typeof(string),
            typeof(Delegate),
            typeof(Action<int>),
            typeof(Func<int>),
            typeof(object)]);

        foreach (var type in types)
        {
            var marshalSize = GetSize(type, Marshal.SizeOf);
            var unsafeSize = UnsafeSizeOf(type);
            var calculatorSize = GetSize(type, SizeCalculator.SizeOf);
            var size = SizeOf(type);
            table.Rows.Add([type.SimpleName(), marshalSize, unsafeSize, calculatorSize, size]);
        }

        output.WriteLine(table.ToString());
        return;

        static string GetSize(Type type, Func<Type, int> getter)
        {
            var (success, size, _, _) = Operate.Execute(() => getter(type));
            return success ? size.ToString() : "-";
        }
    }
}