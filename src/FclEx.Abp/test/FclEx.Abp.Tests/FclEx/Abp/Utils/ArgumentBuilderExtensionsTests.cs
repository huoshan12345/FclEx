namespace FclEx.Abp.Utils;

public class ArgumentBuilderExtensionsTests
{
    public interface IA { }
    public interface IB : IA { }
    public interface IC : IB { }
    public interface IX : IA, IB { }
    public interface IGeneric<T> { }
    public interface IGenericA<T> : IGeneric<T>, IA { }
    public interface IGenericB<T1, T2> : IGenericA<T1>, IA { }

    public class A : IA { }
    public struct S : IA { }
    public class B : A, IB { }
    public class C : B, IC { }
    public class X : B, IA, IB, IC { }
    public class GenericA<T> : IGenericA<T> { }
    public class GenericAObject : IGenericA<object> { }
    public class GenericB<T1, T2> : GenericA<T1>, IGenericB<T1, T2> { }
    public class GenericBObjectInt : GenericB<object, int> { }

    public static IEnumerable<Type> AllTypes { get; } = typeof(ArgumentBuilderExtensionsTests).GetNestedTypes()
        .Concat(new[] { typeof(object), typeof(string), typeof(int), typeof(Type) })
        .Concat(new[] { typeof(IGeneric<>).MakeGenericType(typeof(IGeneric<>)), typeof(GenericA<>).MakeGenericType(typeof(GenericA<>)) });

    public static IEnumerable<object[]> TypePairs { get; } = AllTypes.SelectMany((x, y) => new object[] { x, y });

    [Theory]
    [MemberData(nameof(TypePairs))]
    public void GetInheritDepthFromClassTo_SameType(Type type, Type inheritType)
    {
        if (type.IsClass)
        {
            Assert.Equal(0, type.GetInheritDepthFromClassTo(type));
            if (type == inheritType)
            {
                Assert.Equal(0, type.GetInheritDepthFromClassTo(inheritType));
                Assert.Equal(0, inheritType.GetInheritDepthFromClassTo(type));
            }
        }
    }

    [Theory]
    [MemberData(nameof(TypePairs))]
    public void GetInheritDepthFromClassTo_NonSubType(Type type, Type inheritType)
    {
        if (type.IsClass && !type.IsAssignableFrom(inheritType))
        {
            Assert.Equal(-1, type.GetInheritDepthFromClassTo(inheritType));
        }
    }

    [Theory]
    [InlineData(typeof(object), typeof(A), 1)]
    [InlineData(typeof(object), typeof(B), 2)]
    [InlineData(typeof(object), typeof(C), 3)]
    [InlineData(typeof(A), typeof(B), 1)]
    [InlineData(typeof(A), typeof(C), 2)]
    [InlineData(typeof(B), typeof(C), 1)]
    public void GetInheritDepthFromClassTo_SubType(Type classType, Type inheritType, int depth)
    {
        Assert.Equal(depth, classType.GetInheritDepthFromClassTo(inheritType));
        Assert.Equal(-1, inheritType.GetInheritDepthFromClassTo(classType));
    }

    [Theory]
    [MemberData(nameof(TypePairs))]
    public void GetInheritDepthFromInterfaceTo(Type type, Type inheritType)
    {
        if (type.IsInterface)
        {
            Assert.Equal(0, type.GetInheritDepthFromInterfaceTo(type)); // test for sameType with
            if (type == inheritType)
            {
                Assert.Equal(0, type.GetInheritDepthFromInterfaceTo(inheritType));
                Assert.Equal(0, inheritType.GetInheritDepthFromInterfaceTo(type));
            }
            else if (type.IsAssignableFrom(inheritType))
            {
                Assert.Equal(1, type.GetInheritDepthFromInterfaceTo(inheritType));
                Assert.Equal(-1, inheritType.GetInheritDepthFromInterfaceTo(type));
            }
            else
            {
                Assert.Equal(-1, type.GetInheritDepthFromInterfaceTo(inheritType));
            }
        }
    }

    [Theory]
    [MemberData(nameof(TypePairs))]
    public void GetInheritDepthTo(Type type, Type inheritType)
    {
        Assert.Equal(0, type.GetInheritDepthTo(type)); // test for sameType with
        Assert.Equal(0, inheritType.GetInheritDepthTo(inheritType)); // test for sameType with

        if (type == inheritType)
        {
            Assert.Equal(0, type.GetInheritDepthTo(inheritType));
            Assert.Equal(0, inheritType.GetInheritDepthTo(type));
        }
        else if (type.IsAssignableFrom(inheritType))
        {
            var depth = type.IsInterface ? 1 : type.GetInheritDepthFromClassTo(inheritType);
            Assert.Equal(depth, type.GetInheritDepthTo(inheritType));
            Assert.Equal(-1, inheritType.GetInheritDepthTo(type));
        }
        else
        {
            Assert.Equal(-1, type.GetInheritDepthTo(inheritType));
        }
    }

}