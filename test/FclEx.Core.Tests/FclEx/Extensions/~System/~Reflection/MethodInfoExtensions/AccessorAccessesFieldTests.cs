// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnusedMember.Local
// ReSharper disable ReplaceWithFieldKeyword
// ReSharper disable CollectionNeverUpdated.Local
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace FclEx.Extensions.MethodInfoExtensions;

public class AccessorAccessesFieldTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

    private class Simple
    {
        public int Value { get; set; }
    }

    [Fact]
    public void InstanceGetter_ShouldAccessBackingField()
    {
        var prop = typeof(Simple).GetProperty(nameof(Simple.Value))!;
        var field = typeof(Simple).GetField("<Value>k__BackingField", Flags)!;
        Assert.True(prop.GetMethod!.AccessesField(field));
    }

    [Fact]
    public void InstanceSetter_ShouldAccessBackingField()
    {
        var prop = typeof(Simple).GetProperty(nameof(Simple.Value))!;
        var field = typeof(Simple).GetField("<Value>k__BackingField", Flags)!;
        Assert.True(prop.SetMethod!.AccessesField(field));
    }

    private struct StructType
    {
        public int Value { get; set; }
    }

    [Fact]
    public void StructProperty_ShouldAccessBackingField()
    {
        var prop = typeof(StructType).GetProperty("Value")!;
        var field = typeof(StructType).GetField("<Value>k__BackingField", Flags)!;

        Assert.True(prop.GetMethod!.AccessesField(field));
    }

    private class StaticType
    {
        public static int Value { get; set; }
    }

    [Fact]
    public void StaticProperty_ShouldAccessBackingField()
    {
        var prop = typeof(StaticType).GetProperty("Value", BindingFlags.Public | BindingFlags.Static)!;
        var field = typeof(StaticType).GetField("<Value>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static)!;

        Assert.True(prop.GetMethod!.AccessesField(field));
    }

    private class Generic<T>
    {
        public T Value { get; set; }
    }

    [Fact]
    public void GenericOpenType_ShouldAccessBackingField()
    {
        var prop = typeof(Generic<>).GetProperty("Value")!;
        var field = typeof(Generic<>).GetField("<Value>k__BackingField", Flags)!;

        Assert.True(prop.GetMethod!.AccessesField(field));
    }

    [Fact]
    public void GenericClosedType_ShouldAccessBackingField()
    {
        var prop = typeof(Generic<int>).GetProperty("Value")!;
        var field = typeof(Generic<int>).GetField("<Value>k__BackingField", Flags)!;

        Assert.True(prop.GetMethod!.AccessesField(field));
    }

    private class GenericMixed<T>
    {
        public int Id { get; set; }
    }

    [Fact]
    public void GenericType_NonGenericProperty_ShouldWork()
    {
        var prop = typeof(GenericMixed<int>).GetProperty("Id")!;
        var field = typeof(GenericMixed<int>).GetField("<Id>k__BackingField", Flags)!;

        Assert.True(prop.GetMethod!.AccessesField(field));
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
        var field = typeof(Base).GetField("<Value>k__BackingField", Flags)!;

        Assert.True(prop.GetMethod!.AccessesField(field));
    }

    [Fact]
    public void HashSet_Count_ShouldNotMatchAnyField()
    {
        // public int Count => _count - _freeCount;
        var type = typeof(HashSet<int>);
        var prop = type.GetProperty("Count")!;
        var fields = type.GetFields(Flags);
        var backingFields = new[]
            {
#if NETFRAMEWORK
                "m_count",
#else
                "_count", "_freeCount",
#endif
            }
            .Select(name => type.GetField(name, Flags))
            .ToArray();

        var getter = prop.GetMethod!;

        foreach (var field in fields)
        {
            Assert.Equal(backingFields.Contains(field), getter.AccessesField(field), () => field.Name);
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
    public void IReadOnlyCollection_Implementation_ShouldAccessField()
    {
        var prop = typeof(MyCollection<int>).GetProperty("Count")!;
        var field = typeof(MyCollection<int>).GetField("_list", Flags)!;

        var getter = prop.GetMethod!;

        Assert.True(getter.AccessesField(field));
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

        var field = typeof(ExplicitImpl).GetField("_count", Flags)!;

        Assert.True(prop.AccessesField(field));
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
    public void NonAutoProperty_ShouldAccessField()
    {
        var prop = typeof(NonAuto).GetProperty("Value")!;
        var field = typeof(NonAuto).GetField("_x", Flags)!;

        Assert.True(prop.GetMethod!.AccessesField(field));
        Assert.True(prop.SetMethod!.AccessesField(field));
    }

    private class AddressAccess
    {
        private int _value;

        public ref int GetValueReference() => ref _value;
    }

    [Fact]
    public void MethodUsingLdflda_ShouldAccessTheField()
    {
        var method = typeof(AddressAccess).GetRequiredMethod(nameof(AddressAccess.GetValueReference));
        var field = typeof(AddressAccess).GetField("_value", Flags)!;

        Assert.True(method.AccessesField(field));
    }

    [Fact]
    public void Accessor_WithFieldTokenEmbeddedInOperand_ShouldNotAccessField()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(nameof(Accessor_WithFieldTokenEmbeddedInOperand_ShouldNotAccessField)),
            AssemblyBuilderAccess.Run);
        var typeBuilder = assembly.DefineDynamicModule("main").DefineType("TestType");
        var fieldBuilder = typeBuilder.DefineField("_value", typeof(int), FieldAttributes.Private);
        var getterBuilder = typeBuilder.DefineMethod(
            "get_Item",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            typeof(int),
            [typeof(int), typeof(int)]);
        var propertyBuilder = typeBuilder.DefineProperty("Item", PropertyAttributes.None, typeof(int), [typeof(int), typeof(int)]);
        propertyBuilder.SetGetMethod(getterBuilder);

        var il = getterBuilder.GetILGenerator();
        il.Emit(OpCodes.Ldc_I4, 0x0000017B); // Its operand starts with 0x7B (ldfld).
        il.Emit(OpCodes.Ldarg_2); // Completes the bytes of FieldDef token 0x04000001.
        il.Emit(OpCodes.Pop);
        il.Emit(OpCodes.Ret);

        var type = typeBuilder.CreateType()!;
        var field = type.GetField("_value", Flags)!;
        var getter = type.GetProperty("Item")!.GetMethod!;
        Assert.Equal(0x04000001, field.MetadataToken);

        // The field token occurs inside an ldc.i4 operand; it is not an IL field-access instruction.
        Assert.False(getter.AccessesField(field));
    }
}
