// ReSharper disable PropertyCanBeMadeInitOnly.Local
namespace FclEx.Helpers.ObjectHelperTests;

public class TrySetTests
{
    private class TestClass
    {
        public int IntProp { get; set; }
        public string? StrProp { get; set; }
        public int Field;
        public int ReadOnlyProp { get; } = 42;
        public int InitOnlyProp { get; init; }
    }

    private class BaseClass
    {
        public int Value { get; set; }
    }

    private sealed class DerivedClass : BaseClass;

    private struct TestStruct
    {
        public int Value;
    }

    private struct ContainerStruct
    {
        public TestStruct Inner;
    }

    private class CustomComparer : IEqualityComparer<TestStruct>
    {
        public bool Equals(TestStruct x, TestStruct y)
            => Math.Abs(x.Value - y.Value) < 10;

        public int GetHashCode(TestStruct obj)
            => obj.Value;
    }

    [Fact]
    public void Should_Set_Property_When_Value_Different()
    {
        var obj = new TestClass { IntProp = 1 };
        var result = ObjectHelper.TrySet(obj, x => x.IntProp, 2);

        Assert.True(result);
        Assert.Equal(2, obj.IntProp);
    }

    [Fact]
    public void Should_Not_Set_When_Value_Same()
    {
        var obj = new TestClass { IntProp = 1 };
        var result = ObjectHelper.TrySet(obj, x => x.IntProp, 1);

        Assert.False(result);
        Assert.Equal(1, obj.IntProp);
    }

    [Fact]
    public void Should_Use_Custom_Comparer()
    {
        var obj = new TestClass { StrProp = "abc" };
        var comparer = StringComparer.OrdinalIgnoreCase;
        var result = ObjectHelper.TrySet(obj, x => x.StrProp, "ABC", comparer);

        Assert.False(result);
        Assert.Equal("abc", obj.StrProp);
    }

    [Fact]
    public void Should_Set_Field()
    {
        var obj = new TestClass { Field = 10 };
        var result = ObjectHelper.TrySet(obj, x => x.Field, 20);

        Assert.True(result);
        Assert.Equal(20, obj.Field);
    }

    [Fact]
    public void Should_Throw_When_Property_Is_ReadOnly()
    {
        var obj = new TestClass();

        Assert.Throws<InvalidOperationException>(() =>
            ObjectHelper.TrySet(obj, x => x.ReadOnlyProp, 100));
    }

    [Fact]
    public void Should_Throw_When_Property_Is_InitOnly()
    {
        var obj = new TestClass { InitOnlyProp = 5 };

        Assert.Throws<InvalidOperationException>(() =>
            ObjectHelper.TrySet(obj, x => x.InitOnlyProp, 10));
    }

    [Fact]
    public void Should_Handle_Null_Value()
    {
        var obj = new TestClass { StrProp = "abc" };
        var result = ObjectHelper.TrySet(obj, x => x.StrProp, null);

        Assert.True(result);
        Assert.Null(obj.StrProp);
    }

    [Fact]
    public void Should_Not_Set_When_Both_Null()
    {
        var obj = new TestClass { StrProp = null };
        var result = ObjectHelper.TrySet(obj, x => x.StrProp, null);

        Assert.False(result);
        Assert.Null(obj.StrProp);
    }

    [Fact]
    public void Should_Throw_When_Obj_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ObjectHelper.TrySet<TestClass, int>(null!, x => x.IntProp, 1));
    }

    [Fact]
    public void Should_Throw_When_Selector_Is_Null()
    {
        var obj = new TestClass();

        Assert.Throws<ArgumentNullException>(() =>
            ObjectHelper.TrySet<TestClass, int>(obj, null!, 1));
    }

    [Fact]
    public void Should_Reuse_Cached_Delegates()
    {
        var obj = new TestClass { IntProp = 1 };

        var r1 = ObjectHelper.TrySet(obj, x => x.IntProp, 2);
        var r2 = ObjectHelper.TrySet(obj, x => x.IntProp, 3);
        var r3 = ObjectHelper.TrySet(obj, x => x.IntProp, 3);

        Assert.True(r1);
        Assert.True(r2);
        Assert.False(r3);

        Assert.Equal(3, obj.IntProp);
    }

    [Fact]
    public void Cache_Should_Distinguish_Selector_Target_Types()
    {
        var derived = new DerivedClass { Value = 1 };
        BaseClass asBase = derived;

        Assert.True(ObjectHelper.TrySet(derived, x => x.Value, 2));
        Assert.True(ObjectHelper.TrySet(asBase, x => x.Value, 3));
        Assert.True(ObjectHelper.TrySet(derived, x => x.Value, 4));

        Assert.Equal(4, derived.Value);
    }

    [Fact]
    public void Struct_Should_Update_When_Passed_By_Ref()
    {
        var obj = new TestStruct { Value = 1 };

        var result = ObjectHelper.TrySet(ref obj, x => x.Value, 2);

        Assert.True(result);
        Assert.Equal(2, obj.Value);
    }

    [Fact]
    public void Struct_Should_Update_Multiple_Times()
    {
        var obj = new TestStruct { Value = 1 };

        ObjectHelper.TrySet(ref obj, x => x.Value, 2);
        ObjectHelper.TrySet(ref obj, x => x.Value, 3);

        Assert.Equal(3, obj.Value);
    }

    [Fact]
    public void Struct_Should_Not_Update_When_Value_Same()
    {
        var obj = new TestStruct { Value = 5 };

        var result = ObjectHelper.TrySet(ref obj, x => x.Value, 5);

        Assert.False(result);
        Assert.Equal(5, obj.Value);
    }

    [Fact]
    public void Should_Update_Nested_Struct_Value_When_Ref_To_Inner()
    {
        var obj = new ContainerStruct
        {
            Inner = new TestStruct { Value = 1 }
        };

        var ex = Assert.Throws<ArgumentException>(() => ObjectHelper.TrySet(ref obj, x => x.Inner.Value, 2));
        Assert.Contains("must not reference a nested member", ex.Message);
    }

    [Fact]
    public void Nested_Struct_Should_Update_Inner_Value_From_Inner()
    {
        var obj = new ContainerStruct
        {
            Inner = new TestStruct { Value = 1 }
        };

        var result = ObjectHelper.TrySet(ref obj.Inner, x => x.Value, 2);

        Assert.True(result);
        Assert.Equal(2, obj.Inner.Value);
    }

    [Fact]
    public void Struct_Should_Update_Whole_Field()
    {
        var obj = new ContainerStruct
        {
            Inner = new TestStruct { Value = 1 }
        };

        var newInner = new TestStruct { Value = 99 };

        var result = ObjectHelper.TrySet(ref obj, x => x.Inner, newInner);

        Assert.True(result);
        Assert.Equal(99, obj.Inner.Value);
    }

    [Fact]
    public void Struct_Default_Value_Update()
    {
        var obj = new TestStruct();

        var result = ObjectHelper.TrySet(ref obj, x => x.Value, 10);

        Assert.True(result);
        Assert.Equal(10, obj.Value);
    }

    [Fact]
    public void Struct_Should_Respect_Custom_Comparer()
    {
        var obj = new ContainerStruct
        {
            Inner = new TestStruct { Value = 100 }
        };

        var comparer = new CustomComparer();

        var result = ObjectHelper.TrySet(ref obj, x => x.Inner, new TestStruct { Value = 105 }, comparer);

        Assert.False(result);
        Assert.Equal(100, obj.Inner.Value);
    }
}
