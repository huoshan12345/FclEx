namespace System.Reflection;

public class MemberInfoEqualityComparerTests
{
    private static MemberInfo M(LambdaExpression e)
    {
        var body = e.Body;
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } u)
            body = u.Operand;
        return body switch
        {
            MemberExpression m => m.Member,
            MethodCallExpression mc => mc.Method,
            _ => throw new InvalidOperationException()
        };
    }

    private static MemberInfo M<T>(Expression<Func<T, object>> e)
    {
        return M((LambdaExpression)e);
    }

    private static MemberInfo M<T>(Expression<Action<T>> e)
    {
        return M((LambdaExpression)e);
    }

    [Fact]
    public void MethodOverload()
    {
        var a = typeof(Overloads).GetMethod(nameof(Overloads.Foo), Type.EmptyTypes);
        var b = typeof(Overloads).GetMethod(nameof(Overloads.Foo), [typeof(int)]);
        Assert.False(MemberInfoEqualityComparer.Instance.Equals(a, b));
    }

    [Fact]
    public void GenericMethodConstructed()
    {
        var a = typeof(G<int>).GetMethod(nameof(G<>.Bar))!;
        var b = typeof(G<string>).GetMethod(nameof(G<>.Bar))!;
        Assert.True(MemberInfoEqualityComparer.Instance.Equals(a, b));
    }

    [Fact]
    public void GenericPropertyConstructed()
    {
        var a = M<G<int>>(x => x.Value);
        var b = M<G<string>>(x => x.Value);
        Assert.True(MemberInfoEqualityComparer.Instance.Equals(a, b));
    }

    [Fact]
    public void InterfaceImplementation()
    {
        var a = typeof(IFoo).GetMethod(nameof(IFoo.M))!;
        var b = typeof(CFoo).GetMethod(nameof(CFoo.M))!;
        Assert.False(MemberInfoEqualityComparer.Instance.Equals(a, b));
    }

    [Fact]
    public void Override()
    {
        var a = typeof(Base).GetMethod(nameof(Base.V))!;
        var b = typeof(Derived).GetMethod(nameof(Derived.V))!;
        Assert.False(MemberInfoEqualityComparer.Instance.Equals(a, b));
    }

    [Fact]
    public void NewHidingProperty()
    {
        var a = typeof(A).GetProperty(nameof(A.P))!;
        var b = typeof(B).GetProperty(nameof(B.P))!;
        Assert.False(MemberInfoEqualityComparer.Instance.Equals(a, b));
    }

    [Fact]
    public void ArraySyntheticMethod()
    {
        var a = typeof(int[]).GetMethod("Get");
        var b = typeof(int[]).GetMethod("Set");
        Assert.False(MemberInfoEqualityComparer.Instance.Equals(a, b));
    }

    [Fact]
    public void ExpressionGenericMethod()
    {
        var a = M<G<int>>(x => x.Bar());
        var b = M<G<string>>(x => x.Bar());
        Assert.True(MemberInfoEqualityComparer.Instance.Equals(a, b));
    }

    [Fact]
    public void FieldGenericConstructed()
    {
        var a = typeof(G<int>).GetField(nameof(G<>.Field))!;
        var b = typeof(G<string>).GetField(nameof(G<>.Field))!;
        Assert.True(MemberInfoEqualityComparer.Instance.Equals(a, b));
    }

    [Fact]
    public void StaticGenericProperty()
    {
        var a = typeof(S<int>).GetProperty(nameof(S<>.StaticValue));
        var b = typeof(S<string>).GetProperty(nameof(S<>.StaticValue));
        Assert.True(MemberInfoEqualityComparer.Instance.Equals(a, b));
    }

    [Fact]
    public void GenericMethod_SubstitutedParameter_SameAfterSubstitution()
    {
        // this case depends on what is considered the "definition" of the method.
        // If we consider the method with the concrete parameter type as the definition, then these two methods are different.
        // If we consider the method with the parameter type depending on T as the definition, then these two methods are the same.
        var a = typeof(G<int>).GetMethod(nameof(G<>.Echo));
        var b = typeof(G<string>).GetMethod(nameof(G<>.Echo));
        Assert.True(MemberInfoEqualityComparer.Instance.Equals(a, b));
    }

    [Fact]
    public void GenericMethod_Overload_DependsOnT_AfterSubstitution_NotSame()
    {
        // this case depends on what is considered the "definition" of the method.
        // If we consider the method with the concrete parameter type as the definition, then these two methods are different.
        // If we consider the method with the parameter type depending on T as the definition, then these two methods are the same.
        var a = typeof(G<int>).GetMethod(nameof(G<>.Mix), 1, [typeof(int)]);
        var b = typeof(G<string>).GetMethod(nameof(G<>.Mix), 1, [typeof(string)]);
        Assert.True(MemberInfoEqualityComparer.Instance.Equals(a, b));
    }

    [Fact]
    public void GenericMethod_Overload_ConcreteVsSubstituted_NotSame()
    {
        var a = typeof(G<int>).GetMethod(nameof(G<>.Mix), 1, [typeof(int)]);
        var b = typeof(G<string>).GetMethod(nameof(G<>.Mix), 0, [typeof(int)]);
        Assert.False(MemberInfoEqualityComparer.Instance.Equals(a, b));
    }


    public class Overloads
    {
        public void Foo() { }
        public void Foo(int x) { }
    }

    public interface IFoo
    {
        void M();
    }

    public class CFoo : IFoo
    {
        public void M() { }
    }

    public class Base
    {
        public virtual void V() { }
    }

    public class Derived : Base
    {
        public override void V() { }
    }

    public class A
    {
        public int P { get; set; }
    }

    public class B : A
    {
        public new int P { get; set; }
    }

    public class G<T>
    {
        public T Value { get; set; } = default!;
        public T Field = default!;
        public void Bar() { }
        public T Echo(T x) => x;
        public void Mix(T x) { }
        public void Mix(int x) { }
    }

    public static class S<T>
    {
        public static int StaticValue { get; set; }
    }
}