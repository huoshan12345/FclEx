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

    private void GetAllFieldAddresses_Test<T>(ref T obj, IntPtr[] addresses) where T : notnull
    {
        var type = obj.GetType(); // do not use typeof(T) here.
        var fields = type.GetAllInstanceFields();
        var pointer = Unsafe.AsPointer(ref obj);
        var current = new IntPtr(pointer);
        var offset = type.IsValueType ? 0 : IntPtr.Size;
        foreach (var ((field, address), (prevField, _)) in fields.Zip(addresses).OrderBy(m => m.Second).WithPrevious())
        {
            var size = prevField is null
                ? offset
                : UnsafeHelper.SizeOf(prevField.FieldType);
            current += size.RoundUp(IntPtr.Size);
            output.WriteLine("Field: " + field.GetAutoPropertyNameOrFieldName());
            Assert.Equal(current.ToHexString(), address.ToHexString());
        }
    }

    [Fact]
    public void GetAllFieldAddresses_Class_Test()
    {
        var obj = new TestClass();
        var addresses = ObjectAccessor.GetAllFieldAddresses(ref obj);
        GetAllFieldAddresses_Test(ref obj, addresses);
    }

    [Fact]
    public void GetAllFieldAddresses_Struct_Test()
    {
        var obj = new TestStruct();
        var addresses = ObjectAccessor.GetAllFieldAddresses(ref obj);
        GetAllFieldAddresses_Test(ref obj, addresses);
    }

    [Fact]
    public void GetAllFieldAddresses_Type_Class_Test()
    {
        object obj = new TestClass();
        var addresses = ObjectAccessor.GetAllFieldAddresses(ref obj, obj.GetType());
        GetAllFieldAddresses_Test(ref obj, addresses);
    }

    [Fact]
    public void GetAllFieldAddresses_Type_Struct_Test()
    {
        object obj = new TestStruct();
        var addresses = ObjectAccessor.GetAllFieldAddresses(ref obj, obj.GetType());
        GetAllFieldAddresses_Test(ref obj, addresses);
    }

}