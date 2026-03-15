// ReSharper disable UnusedMember.Local
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnusedMember.Global
namespace FclEx.Extensions.Reflection;

public class TestClass
{
    public int AutoProperty { get; set; }

    public int ReadOnlyAuto { get; }

    public int InitOnly { get; init; }

    public static int StaticAuto { get; set; }

    public int NormalProperty
    {
        get => _field;
        set => _field = value;
    }

    private int _field;
}

public struct TestStruct
{
    public int Value { get; set; }
}

public class BaseClass
{
    public int BaseAuto { get; set; }
}

public class DerivedClass : BaseClass;

public class GenericClass<T>
{
    public T? Value { get; set; }
    public static T? StaticValue { get; set; }
    public int Id { get; set; }
    public static int StaticId { get; set; }
}

public struct GenericStruct<T>
{
    public T Value { get; set; }
    public int Id { get; set; }
    public static int StaticId { get; set; }
}
