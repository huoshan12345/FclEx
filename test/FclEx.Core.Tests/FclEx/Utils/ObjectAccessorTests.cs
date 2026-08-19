using FclEx.TestModels;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Sdk;

namespace FclEx.Utils;

public class ObjectAccessorTests
{
    [Fact]
    public unsafe void GetAddress_Null_Test()
    {
        var obj = default(object?);
        var expected = new IntPtr(&obj);
        var actual = ObjectAccessor.GetAddress(ref obj);
        Assert.Equal(expected.ToHexString(), actual.ToHexString());
    }

    [Fact]
    public unsafe void GetAddress_Class_Test()
    {
        var obj = new object();
        var expected = new IntPtr(&obj);
        var actual = ObjectAccessor.GetAddress(ref obj);
        Assert.Equal(expected.ToHexString(), actual.ToHexString());
    }

    [Fact]
    public unsafe void GetAddress_Struct_Test()
    {
        var obj = new CommonStruct();
        var expected = new IntPtr(&obj);
        var actual = ObjectAccessor.GetAddress(ref obj);
        Assert.Equal(expected.ToHexString(), actual.ToHexString());
    }

    // Prefer IntPtr over nint here to avoid member resolution issues in .NET Framework.
    private static void GetAllFieldAddresses_Test<T>(ref T obj, IReadOnlyList<IntPtr> addresses) where T : notnull
    {
        GC.Collect(); // test movable object.

        var type = obj.GetType(); // do not use typeof(T) here.
        var fields = type.GetAllInstanceFields();

        var table = new ConsoleTable(new() { Columns = ["Name", "Type", "Address", "Offset", "Value"], RenderColumns = true });

        foreach (var ((field, address), (_, prevAddr)) in fields.Zip(addresses).OrderBy(m => m.Second.ToInt64()).WithPrevious())
        {
            var value = Unsafe.GetValue(address, field.FieldType);
            var expectedValue = field.GetValue(obj);

            try
            {
                Assert.Equal(expectedValue, value);
            }
            catch (EqualException ex)
            {
                ex.SetMessage(e => $"Field '{field.Name}': " + e.Message).ReThrow();
            }

            var name = field.GetAutoPropertyOrFieldName();
            var offset = prevAddr == IntPtr.Zero ? 0 : address.Subtract(prevAddr);
            table.AddRow([name, field.FieldType.ShortName(), address.ToHexString(), offset, expectedValue]);
        }

        if (TestHelper.IsRunningUnderReSharper())
        {
            TestContext.Current.TestOutputHelper?.WriteLine(table.ToString());
        }
    }

    [Fact]
    public void GetAllFieldAddresses_Class_Test()
    {
        var random = new Random(0);
        var obj = new BlittableClass
        {
            Double = random.NextDouble(),
            Int = random.Next(),
        };

        using var _ = GCHandle.Create(obj, GCHandleType.Pinned);
        var addresses = ObjectAccessor.GetAllFieldAddresses(ref obj);
        GetAllFieldAddresses_Test(ref obj, addresses);
    }

    [Fact]
    public void GetAllFieldAddresses_Struct_Test()
    {
        var random = new Random(0);
        var obj = new BlittableStruct
        {
            Double = random.NextDouble(),
            Int = random.Next(),
        };

        var addresses = ObjectAccessor.GetAllFieldAddresses(ref obj);
        GetAllFieldAddresses_Test(ref obj, addresses);
    }

    [Fact]
    public void GetAllFieldAddresses_Class_Type_Test()
    {
        var random = new Random(0);
        object obj = new BlittableClass
        {
            Double = random.NextDouble(),
            Int = random.Next(),
        };

        using var _ = GCHandle.Create(obj, GCHandleType.Pinned);
        var addresses = ObjectAccessor.GetAllFieldAddresses(ref obj, obj.GetType());
        GetAllFieldAddresses_Test(ref obj, addresses);
    }

    [Fact]
    public void GetAllFieldAddresses_Struct_Type_Test()
    {
        var random = new Random(0);
        object obj = new BlittableStruct
        {
            Double = random.NextDouble(),
            Int = random.Next(),
        };

        using var _ = GCHandle.Create(obj, GCHandleType.Pinned);
        var addresses = ObjectAccessor.GetAllFieldAddresses(ref obj, obj.GetType());
        GetAllFieldAddresses_Test(ref obj, addresses);
    }
}