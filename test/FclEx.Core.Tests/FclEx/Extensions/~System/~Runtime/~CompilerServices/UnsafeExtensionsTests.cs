using FclEx.TestModels;

namespace FclEx.Extensions;

public class UnsafeExtensionsTests
{
    public static readonly ReadOnlyHashSet<Type> CommonValueTypes =
    [
        ..Types.BlittableTypes,
        typeof(DateTime), // non-blittable
        typeof(DateTimeOffset), // non-blittable
#if NET8_0_OR_GREATER
        typeof(DateOnly), // blittable
        typeof(TimeOnly), // blittable        
#endif
        typeof(ValueTuple<int>), // non-blittable
        typeof(ValueTuple<DateTimeOffset, int, DateTime>), // non-blittable
    ];

    public static readonly TheoryData<Type> BuiltInValueTypeCases = CommonValueTypes.ToTheoryData();

    private static readonly MethodInfo _sizeOfTTest = typeof(UnsafeExtensionsTests).GetRequiredMethod(nameof(SizeOf_T_Test), 1);
    private static readonly MethodInfo _sizeOf = typeof(UnsafeExtensions).GetRequiredMethod(nameof(UnsafeExtensions.SizeOf));

    [Theory]
    [MemberData(nameof(BuiltInValueTypeCases))]
    public void SizeOf_BuiltInValueType_Test(Type type)
    {
        _sizeOfTTest.MakeGenericMethod(type).Invoke<int>(null, null);
    }

    [Fact]
    public void SizeOf_Struct_Test()
    {
        Assert.Equal(Unsafe.SizeOf<CommonStruct>(), Unsafe.SizeOf<CommonStruct>());
    }

    [Fact]
    public void GetValue_ReadsUnmanagedValue()
    {
        using var memory = Marshal.AllocHGlobalDisposable(sizeof(int));
        Marshal.WriteInt32(memory.Value, 42);

        Assert.Equal(42, Unsafe.GetValue<int>(memory.Value));
    }

    [Fact]
    public void GetValue_WithRuntimeManagedType_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => Unsafe.GetValue(IntPtr.Zero, typeof(string)));
    }

    [Theory]
    [InlineData(typeof(object))]
    [InlineData(typeof(string))]
    [InlineData(typeof(List<string>))]
    [InlineData(typeof(List<int>))]
    public unsafe void SizeOf_ReferenceType_Test(Type type)
    {
        var size = Unsafe.SizeOf(type);
        Assert.Equal(sizeof(IntPtr), size); // 8 for 64-bit
    }

    [Theory]
    [InlineData(typeof(List<>))]
    [InlineData(typeof(Dictionary<,>))]
    public void SizeOf_OpenGeneric_Test(Type type)
    {
        Assert.Throws<InvalidOperationException>(() => Unsafe.SizeOf(type));
    }

    private static unsafe int SizeOf_T_Test<T>()
    {
        var size = sizeof(T);
        Assert.Equal(size, Unsafe.SizeOf<T>());
        return size;
    }

    [LocalOnlyFact]
    public void CompareSizeOf()
    {
        var table = new ConsoleTable(new()
        {
            Columns = ["Type", nameof(Marshal), nameof(Unsafe)],
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
            var size = Unsafe.SizeOf(type);
            table.AddRow([type.ShortName(), marshalSize, size]);
        }

        if (TestHelper.IsRunningUnderReSharper())
        {
            TestContext.Current.TestOutputHelper?.WriteLine(table.ToString());
        }

        static string GetSize(Type type, Func<Type, int> getter)
        {
            var (success, size, _, _) = Operation.Execute(() => getter(type));
            return success ? size.ToString() : "-";
        }
    }
}
