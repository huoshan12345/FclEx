using FclEx.TestModels;

namespace FclEx.Helpers;

public class UnsafeHelperTests(ITestOutputHelper output)
{
    public static readonly ReadOnlySet<Type> CommonValueTypes =
    [
        ..Types.BlittableTypes,
        typeof(DateTime), // non-blittable
        typeof(DateTimeOffset), // non-blittable
        typeof(DateOnly), // blittable
        typeof(TimeOnly), // blittable
        typeof(ValueTuple<int>), // non-blittable
        typeof(ValueTuple<DateTimeOffset, int, DateTime>), // non-blittable
    ];

    public static readonly IEnumerable<object[]> BuiltInValueTypeCases = CommonValueTypes
        .Select(m => new object[] { m });

    private static readonly MethodInfo _sizeOfTTest = typeof(UnsafeHelperTests).GetRequiredMethod(nameof(SizeOf_T_Test), 1);
    private static readonly MethodInfo _sizeOf = typeof(UnsafeHelper).GetRequiredMethod(nameof(UnsafeHelper.SizeOf), 1);
    private static readonly MethodInfo _unsafeSizeOf = typeof(Unsafe).GetRequiredMethod(nameof(Unsafe.SizeOf), 1);

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
        Assert.Equal(Unsafe.SizeOf<CommonStruct>(), UnsafeHelper.SizeOf<CommonStruct>());
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

    [LocalOnlyFact]
    public void CompareSizeOf()
    {
        var table = new ConsoleTable(new()
        {
            Columns = ["Type", nameof(Marshal), nameof(Unsafe), nameof(SizeCalculator), nameof(UnsafeHelper)],
            RenderColumns = true,
        });

        var types = CommonValueTypes.Concat([
            typeof(CommonClass),
            typeof(CommonStruct),
            typeof(CommonRecord),
            typeof(CommonRecordStruct),
            typeof(EmptyClass),
            typeof(EmptyStruct),
            typeof(EmptyRecord),
            typeof(EmptyRecordStruct),
            typeof(object),
            typeof(string),
            typeof(TextWriter), // abstract class
            typeof(Delegate),
            typeof(Action<int>),
            typeof(Func<int, long>)]);

        foreach (var type in types)
        {
            var marshalSize = GetSize(type, Marshal.SizeOf);
            var unsafeSize = UnsafeSizeOf(type);
            var calculatorSize = GetSize(type, SizeCalculator.SizeOf);
            var size = SizeOf(type);
            table.Rows.Add([type.ShortName(), marshalSize, unsafeSize, calculatorSize, size]);
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