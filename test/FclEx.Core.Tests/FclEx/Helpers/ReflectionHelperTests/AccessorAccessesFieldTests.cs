// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnusedMember.Local
// ReSharper disable ReplaceWithFieldKeyword
// ReSharper disable CollectionNeverUpdated.Local
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

using static FclEx.Helpers.ReflectionHelper;

namespace FclEx.Helpers.ReflectionHelperTests;

public class AccessorAccessesFieldTests
{
    private class Simple
    {
        public int Value { get; set; }
    }

    [Fact]
    public void InstanceGetter_ShouldAccessBackingField()
    {
        var prop = typeof(Simple).GetProperty(nameof(Simple.Value))!;
        var field = typeof(Simple).GetField("<Value>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var getter = prop.GetMethod!;

        Assert.True(AccessorAccessesField(getter, field));
    }

    [Fact]
    public void InstanceSetter_ShouldAccessBackingField()
    {
        var prop = typeof(Simple).GetProperty(nameof(Simple.Value))!;
        var field = typeof(Simple).GetField("<Value>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var setter = prop.SetMethod!;

        Assert.True(AccessorAccessesField(setter, field));
    }

    private struct StructType
    {
        public int Value { get; set; }
    }

    [Fact]
    public void StructProperty_ShouldAccessBackingField()
    {
        var prop = typeof(StructType).GetProperty("Value")!;
        var field = typeof(StructType).GetField("<Value>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        Assert.True(AccessorAccessesField(prop.GetMethod!, field));
    }

    private class StaticType
    {
        public static int Value { get; set; }
    }

    [Fact]
    public void StaticProperty_ShouldAccessBackingField()
    {
        var prop = typeof(StaticType).GetProperty("Value",
            BindingFlags.Public | BindingFlags.Static)!;

        var field = typeof(StaticType).GetField("<Value>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        Assert.True(AccessorAccessesField(prop.GetMethod!, field));
    }

    private class Generic<T>
    {
        public T Value { get; set; }
    }

    [Fact]
    public void GenericOpenType_ShouldAccessBackingField()
    {
        var prop = typeof(Generic<>).GetProperty("Value")!;
        var field = typeof(Generic<>).GetField("<Value>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        Assert.True(AccessorAccessesField(prop.GetMethod!, field));
    }

    [Fact]
    public void GenericClosedType_ShouldAccessBackingField()
    {
        var prop = typeof(Generic<int>).GetProperty("Value")!;
        var field = typeof(Generic<int>).GetField("<Value>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        Assert.True(AccessorAccessesField(prop.GetMethod!, field));
    }

    private class GenericMixed<T>
    {
        public int Id { get; set; }
    }

    [Fact]
    public void GenericType_NonGenericProperty_ShouldWork()
    {
        var prop = typeof(GenericMixed<int>).GetProperty("Id")!;
        var field = typeof(GenericMixed<int>).GetField("<Id>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        Assert.True(AccessorAccessesField(prop.GetMethod!, field));
    }

    private class Base
    {
        public int Value { get; set; }
    }

    private class Derived : Base
    {
    }

    [Fact]
    public void InheritedProperty_ShouldAccessBaseBackingField()
    {
        var prop = typeof(Derived).GetProperty("Value")!;
        var field = typeof(Base).GetField("<Value>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        Assert.True(AccessorAccessesField(prop.GetMethod!, field));
    }

    [Fact]
    public void HashSet_Count_ShouldNotMatchAnyField()
    {
        var prop = typeof(HashSet<int>).GetProperty("Count")!;
        var fields = typeof(HashSet<int>).GetFields(
            BindingFlags.NonPublic | BindingFlags.Instance);

        var getter = prop.GetMethod!;

        // HashSet.Count 不是 auto-property（它有逻辑）
        foreach (var field in fields)
        {
            Assert.False(AccessorAccessesField(getter, field));
        }
    }

    private class MyCollection<T> : IReadOnlyCollection<T>
    {
        private readonly List<T> _list = [];

        public int Count => _list.Count;

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Fact]
    public void IReadOnlyCollection_Implementation_ShouldNotMatchBackingField()
    {
        var prop = typeof(MyCollection<int>).GetProperty("Count")!;
        var field = typeof(MyCollection<int>).GetField("_list",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var getter = prop.GetMethod!;

        Assert.False(AccessorAccessesField(getter, field));
    }

    private class ExplicitImpl : IReadOnlyCollection<int>
    {
        private readonly int _count;

        int IReadOnlyCollection<int>.Count => _count;

        public IEnumerator<int> GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Fact]
    public void ExplicitInterfaceProperty_ShouldAccessField()
    {
        var prop = typeof(ExplicitImpl)
            .GetInterfaceMap(typeof(IReadOnlyCollection<int>))
            .TargetMethods
            .First(m => m.Name.Contains("Count"));

        var field = typeof(ExplicitImpl).GetField("_count",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        Assert.True(AccessorAccessesField(prop, field));
    }

    private class NonAuto
    {
        private int _x;

        public int Value
        {
            get => _x + 1;
            set => _x = value;
        }
    }

    [Fact]
    public void NonAutoProperty_ShouldNotMatchBackingField()
    {
        var prop = typeof(NonAuto).GetProperty("Value")!;
        var field = typeof(NonAuto).GetField("_x",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        Assert.False(AccessorAccessesField(prop.GetMethod!, field));
    }
}