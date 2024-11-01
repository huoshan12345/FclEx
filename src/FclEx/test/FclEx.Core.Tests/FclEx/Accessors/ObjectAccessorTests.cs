using FclEx.TestModels;

namespace FclEx.Accessors;

public unsafe class ObjectAccessorTests(ITestOutputHelper output)
{
    [Fact]
    public void GetInstanceAddress_Class_Test()
    {
        var obj = new object();
        var expected = new IntPtr(&obj);
        var actual = ObjectAccessor.GetInstanceAddress(ref obj);
        Assert.Equal(expected.ToHexString(), actual.ToHexString());
    }

    [Fact]
    public void GetInstanceAddress_Struct_Test()
    {
        var obj = new TestStruct();
        var expected = new IntPtr(&obj);
        var actual = ObjectAccessor.GetInstanceAddress(ref obj);
        Assert.Equal(expected.ToHexString(), actual.ToHexString());
    }

    [Fact]
    public void GetAllFieldAddresses_Class_Test()
    {
        var obj = new TestClass();
        var fields = obj.GetType().GetAllInstanceFields();
        var addresses = ObjectAccessor.GetAllFieldAddresses(ref obj);
        Assert.Equal(fields.Length, addresses.Length);

        var current = new IntPtr(&obj);
        foreach (var ((field, address), (prevField, _)) in fields.Zip(addresses).OrderBy(m => m.Second).WithPrevious())
        {
            var size = prevField is null
                ? IntPtr.Size
                : UnsafeHelper.SizeOf(prevField.FieldType);
            current += size.RoundUp(IntPtr.Size);
            output.WriteLine("Field: " + field.GetAutoPropertyNameOrFieldName());
            Assert.Equal(current.ToHexString(), address.ToHexString());
        }
    }

    [Fact]
    public void GetAllFieldAddresses_Struct_Test()
    {
        var obj = new TestStruct();
        var fields = obj.GetType().GetAllInstanceFields();
        var addresses = ObjectAccessor.GetAllFieldAddresses(ref obj);
        Assert.Equal(fields.Length, addresses.Length);

        var current = new IntPtr(&obj);
        foreach (var ((field, address), (prevField, _)) in fields.Zip(addresses).OrderBy(m => m.Second).WithPrevious())
        {
            var size = prevField is null
                ? 0
                : UnsafeHelper.SizeOf(prevField.FieldType);
            current += size.RoundUp(IntPtr.Size);
            output.WriteLine("Field: " + field.GetAutoPropertyNameOrFieldName());
            Assert.Equal(current.ToHexString(), address.ToHexString());
        }
    }

}