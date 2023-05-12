using System;
using System.Linq.Expressions;
using FclEx.Testers;

namespace FclEx.Helpers;

public class ErasureTypeTests
{
    [Fact]
    public void ErasureType_Int_Test()
    {
        var obj = new Person() { Age = 10 };
        Expression<Func<Person, int>> exp = m => m.Age;
        var unTypedExp = ExpressionHelper.ErasureType(exp);
        var actual = unTypedExp.Compile()(obj);
        Assert.IsType<int>(actual);
        Assert.Equal(obj.Age, actual);
    }

    [Fact]
    public void ErasureType_String_Test()
    {
        var obj = new Person() { Name = nameof(ErasureType_String_Test) };
        Expression<Func<Person, string>> exp = m => m.Name;
        var unTypedExp = ExpressionHelper.ErasureType(exp);
        var actual = unTypedExp.Compile()(obj);
        Assert.IsType<string>(actual);
        Assert.Equal(obj.Name, (string)actual);
    }

    [Fact]
    public void ErasureType_Object_Test()
    {
        var o = new object();
        var obj = new Person() { Obj = o };
        Expression<Func<Person, object>> exp = m => m.Obj;
        var unTypedExp = ExpressionHelper.ErasureType(exp);
        var actual = unTypedExp.Compile()(obj);
        Assert.IsType<object>(actual);
        Assert.Equal(o, actual);
        Assert.Same(o, actual);
    }

    [Fact]
    public void ErasureType_Object_Int_Test()
    {
        object o = 10;
        var obj = new Person() { Obj = o };
        Expression<Func<Person, object>> exp = m => m.Obj;
        var unTypedExp = ExpressionHelper.ErasureType(exp);
        var actual = unTypedExp.Compile()(obj);
        Assert.IsType<int>(actual);
        Assert.Equal((int)o, (int)actual);
    }
}