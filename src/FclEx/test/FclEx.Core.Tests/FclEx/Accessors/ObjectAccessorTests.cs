using FclEx.TestModels;

namespace FclEx.Accessors;

public unsafe class ObjectAccessorTests(ITestOutputHelper output)
{
    [Fact]
    public void GetAddress_Null_Test()
    {
        var obj = default(object?);
        var expected = new IntPtr(&obj);
        var actual = ObjectAccessor.GetAddress(ref obj);
        Assert.Equal(expected.ToHexString(), actual.ToHexString());
    }

    [Fact]
    public void GetAddress_Class_Test()
    {
        var obj = new object();
        var expected = new IntPtr(&obj);
        var actual = ObjectAccessor.GetAddress(ref obj);
        Assert.Equal(expected.ToHexString(), actual.ToHexString());
    }

    [Fact]
    public void GetAddress_Struct_Test()
    {
        var obj = new TestStruct();
        var expected = new IntPtr(&obj);
        var actual = ObjectAccessor.GetAddress(ref obj);
        Assert.Equal(expected.ToHexString(), actual.ToHexString());
    }

    private void GetAllFieldAddresses_Test<T>(ref T obj, IntPtr[] addresses) where T : notnull
    {
        var type = obj.GetType(); // do not use typeof(T) here.
        var fields = type.GetAllInstanceFields();

        var baseAddress = ObjectAccessor.GetFirstFieldAddress(ref obj);
        var str = UnsafeHelper.GetValue<string>(baseAddress);

        var table = new ConsoleTable(new() { Columns = ["Name", "Type", "Address", "Offset", "Value"], RenderColumns = true });

        foreach (var ((field, address), (_, prevAddr)) in fields.Zip(addresses).OrderBy(m => m.Second).WithPrevious())
        {
            var value = UnsafeHelper.GetValue(address, field.FieldType);
            var expectedValue = field.GetValue(obj);
            Assert.Equal(expectedValue, value);

            var name = field.GetAutoPropertyOrFieldName();
            var offset = prevAddr == IntPtr.Zero ? 0 : address.Subtract(prevAddr);
            table.Rows.Add([name, field.FieldType.ShortName(), address.ToHexString(), offset, expectedValue]);
        }

        output.WriteLine(table.ToString());
    }

    [Fact]
    public void GetAllFieldAddresses_Class_Test()
    {
        var random = new Random(0);
        var obj = new TestClass
        {
            DateTime = random.NextDateTime(),
            Int = random.Next(),
            String = random.NextString(10),
        };
        var addresses = ObjectAccessor.GetAllFieldAddresses(ref obj);
        GetAllFieldAddresses_Test(ref obj, addresses);
    }

    [Fact]
    public void GetAllFieldAddresses_Struct_Test()
    {
        var random = new Random(0);
        var obj = new TestStruct
        {
            DateTime = random.NextDateTime(),
            Int = random.Next(),
            String = random.NextString(10),
        };
        var addresses = ObjectAccessor.GetAllFieldAddresses(ref obj);
        GetAllFieldAddresses_Test(ref obj, addresses);
    }
    
    [Fact]
    public void GetAllFieldAddresses_Class_Type_Test()
    {
        var random = new Random(0);
        object obj = new TestClass
        {
            DateTime = random.NextDateTime(),
            Int = random.Next(),
            String = random.NextString(10),
        };
        var addresses = ObjectAccessor.GetAllFieldAddresses(ref obj, obj.GetType());
        GetAllFieldAddresses_Test(ref obj, addresses);
    }

    [Fact]
    public void GetAllFieldAddresses_Struct_Type_Test()
    {
        var random = new Random(0);
        object obj = new TestStruct
        {
            DateTime = random.NextDateTime(),
            Int = random.Next(),
            String = random.NextString(10),
        };

        var addresses = ObjectAccessor.GetAllFieldAddresses(ref obj, obj.GetType());
        GetAllFieldAddresses_Test(ref obj, addresses);
    }
}