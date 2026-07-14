using System.Reflection.Emit;

namespace FclEx.Extensions.Reflection.TypeExtensions;

[CompilerGenerated]
public class CompilerGeneratedMarkedType;

public class TypeInfoExTests
{
    [Fact]
    public void GetTypeInfoEx_ShouldReturnCachedTypeFacts()
    {
        var info = typeof(Dictionary<string, int>).GetTypeInfoEx();
        var cached = typeof(Dictionary<string, int>).GetTypeInfoEx();

        Assert.Same(info, cached);
        Assert.Equal(typeof(Dictionary<string, int>), info.Type);
        Assert.Equal("Dictionary", info.SimpleName);
        Assert.Equal("Dictionary<String, Int32>", info.ShortName);
        Assert.Equal("System.Collections.Generic.Dictionary<String, Int32>", info.LongName);
        Assert.False(info.IsNullable);
        Assert.True(info.IsEnumerable);
        Assert.Equal(typeof(KeyValuePair<string, int>), info.EnumerableElementType);
    }

    [Fact]
    public void IsDynamic_ShouldReturnTrue_WhenTypeHasDynamicAttribute()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynamicAttributeTestAssembly"), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Main");
        var typeBuilder = module.DefineType("DynamicMarkedTypeForTest", TypeAttributes.Public);
        var attributeCtor = typeof(DynamicAttribute).GetConstructor(Type.EmptyTypes)!;
        typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(attributeCtor, []));
        var type = typeBuilder.CreateTypeInfo()!.AsType();

        Assert.True(type.IsDynamic());
        Assert.False(typeof(TypeInfoExTests).IsDynamic());
    }

    [Fact]
    public void IsCompilerGenerated_ShouldReturnTrue_WhenTypeHasCompilerGeneratedAttribute()
    {
        Assert.True(typeof(CompilerGeneratedMarkedType).IsCompilerGenerated());
        Assert.False(typeof(TypeInfoExTests).IsCompilerGenerated());
    }

    [Fact]
    public void GetTypeCode_ShouldReturnSystemTypeCode()
    {
        Assert.Equal(TypeCode.Int32, typeof(int).GetTypeCode());
        Assert.Equal(TypeCode.String, typeof(string).GetTypeCode());
        Assert.Equal(TypeCode.Object, typeof(TypeInfoExTests).GetTypeCode());
    }
}
