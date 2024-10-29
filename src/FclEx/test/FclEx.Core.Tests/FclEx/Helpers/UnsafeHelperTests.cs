using System.Runtime.InteropServices;

namespace FclEx.Helpers;

public class UnsafeHelperTests(ITestOutputHelper output)
{
    public struct TestStruct
    {
        public int Int { get; set; }
        public DateTime DateTime { get; set; }
    }

    public record TestRecord(int Int, long Long);

    public static readonly Type[] BuiltInValueTypes =
    [
        typeof(bool),
        typeof(char),
        typeof(sbyte),
        typeof(byte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(DateTime),
        typeof(TimeSpan),
        typeof(Guid),
        typeof(DateTimeOffset),
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(IntPtr),
        typeof(UIntPtr),
        typeof(ValueTuple<int>),
        typeof(ValueTuple<int, long, DateTimeOffset, DateTime>),
    ];

    public static readonly IEnumerable<object[]> BuiltInValueTypeCases = BuiltInValueTypes.Select(m => new object[] { m });

    private static readonly MethodInfo _sizeOfTTest = typeof(UnsafeHelperTests).GetRequiredMethod(nameof(SizeOf_T_Test));
    private static readonly MethodInfo _sizeOf = typeof(UnsafeHelper).GetRequiredMethod(nameof(UnsafeHelper.SizeOf));
    private static readonly MethodInfo _sizeOf2 = typeof(UnsafeHelper).GetRequiredMethod(nameof(UnsafeHelper.SizeOf2));

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
        Assert.Equal(16, UnsafeHelper.SizeOf<TestStruct>());
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

    private static int SizeOf2(Type type)
    {
        return _sizeOf2.MakeGenericMethod(type).Invoke<int>(null, null);
    }

    [Fact]
    public void CompareSizeOf()
    {
        var table = new ConsoleTable(new()
        {
            Columns = ["Type", $"{nameof(Marshal)}", $"{nameof(UnsafeHelper.SizeOf)}", $"{nameof(UnsafeHelper.SizeOf2)}"],
            RenderColumns = true,
        });

        var types = BuiltInValueTypes.Concat([
            typeof(TestStruct),
            typeof(TestRecord),
            typeof(string),
            typeof(object)]);

        foreach (var type in types)
        {
            var size = SizeOf(type);
            var (success, marshalSize, _, _) = Operate.Execute(() => Marshal.SizeOf(type));
            var marshalSizeStr = success ? marshalSize.ToString() : "-";
            var size2 = SizeOf2(type);
            table.Rows.Add([type.SimpleName(), marshalSizeStr, size, size2]);
        }

        output.WriteLine(table.ToString());
    }
}