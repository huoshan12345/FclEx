using System.Reflection.Emit;

namespace FclEx.Extensions.Reflection.TypeExtensions;

[CompilerGenerated]
public class CompilerGeneratedMarkedType;

public class GenericOuter<T>
{
    public class GenericInner<TInner>;
    public class NonGenericInner;

    public class GenericMiddle<TMiddle>
    {
        public class GenericInnerMost<TInner>;
    }
}

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
        Assert.Equal(typeof(KeyValuePair<string, int>), info.EnumerableElementTypes.Single());
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

    [Fact]
    public void NestedGenericTypeNames_ShouldAssignArgumentsToTheirDeclaringTypes()
    {
        var type = typeof(GenericOuter<int>.GenericInner<string>);

        Assert.Equal("GenericInner<String>", type.ShortName());
        Assert.Equal(
            "FclEx.Extensions.Reflection.TypeExtensions.GenericOuter<Int32>.GenericInner<String>",
            type.LongName());
    }

    [Fact]
    public void NestedTypeWithOnlyOuterGenericArguments_ShouldNotRepeatThoseArguments()
    {
        var type = typeof(GenericOuter<int>.NonGenericInner);

        Assert.Equal("NonGenericInner", type.ShortName());
        Assert.Equal(
            "FclEx.Extensions.Reflection.TypeExtensions.GenericOuter<Int32>.NonGenericInner",
            type.LongName());
    }

    [Fact]
    public void FrameworkNestedType_ShouldFormatClosedOuterGenericArguments()
    {
        var type = typeof(Dictionary<string, int>.Enumerator);

        Assert.Equal("Enumerator", type.ShortName());
        Assert.Equal("System.Collections.Generic.Dictionary<String, Int32>.Enumerator", type.LongName());
    }

    [Fact]
    public void OpenNestedGenericShortName_ShouldAssignInnerParameterOnce()
    {
        var type = typeof(GenericOuter<>.GenericInner<>);

        Assert.Equal("GenericInner<TInner>", type.ShortName());
    }

    [Fact]
    public void OpenNestedGenericLongName_ShouldAssignEachParameterOnce()
    {
        var type = typeof(GenericOuter<>.GenericInner<>);

        Assert.Equal(
            "FclEx.Extensions.Reflection.TypeExtensions.GenericOuter<T>.GenericInner<TInner>",
            type.LongName());
    }

    [Fact]
    public void OpenNestedGenericParameterLongNames_ShouldNotReenterDeclaringTypeFormatting()
    {
        var genericParameters = typeof(GenericOuter<>.GenericInner<>).GetGenericArguments();

        Assert.Equal(
            "FclEx.Extensions.Reflection.TypeExtensions.GenericOuter<T>.GenericInner<TInner>.T",
            genericParameters[0].LongName());
        Assert.Equal(
            "FclEx.Extensions.Reflection.TypeExtensions.GenericOuter<T>.GenericInner<TInner>.TInner",
            genericParameters[1].LongName());
    }

    [Fact]
    public void MultiLevelNestedGenericTypeNames_ShouldAssignEachArgumentOnce()
    {
        var type = typeof(GenericOuter<int>.GenericMiddle<long>.GenericInnerMost<string>);

        Assert.Equal("GenericInnerMost<String>", type.ShortName());
        Assert.Equal(
            "FclEx.Extensions.Reflection.TypeExtensions.GenericOuter<Int32>.GenericMiddle<Int64>.GenericInnerMost<String>",
            type.LongName());
    }

    [Fact]
    public void ArrayOfNestedGenericTypeNames_ShouldPreserveArrayShapeWithoutRepeatingArguments()
    {
        var type = typeof(GenericOuter<int>.GenericInner<string>[,]);

        Assert.Equal("GenericInner<String>[,]", type.ShortName());
        Assert.Equal(
            "FclEx.Extensions.Reflection.TypeExtensions.GenericOuter<Int32>.GenericInner<String>[,]",
            type.LongName());
    }

    [Fact]
    public void NonVectorArrayTypeNames_ShouldPreserveArrayShape()
    {
        var type = typeof(GenericOuter<int>.GenericInner<string>).MakeArrayType(1);

        Assert.Equal("GenericInner<String>[*]", type.ShortName());
        Assert.Equal(
            "FclEx.Extensions.Reflection.TypeExtensions.GenericOuter<Int32>.GenericInner<String>[*]",
            type.LongName());
    }

    [Fact]
    public void TypeInfoEx_PublicState_ShouldBeReadOnly()
    {
        var fields = typeof(TypeInfoEx).GetFields(BindingFlags.Instance | BindingFlags.Public);

        Assert.NotEmpty(fields);
        Assert.All(fields, field => Assert.True(field.IsInitOnly, field.Name));
        Assert.True(typeof(TypeInfoEx).IsSealed);
    }

    [Fact]
    public void GetTypeInfoEx_ShouldNotKeepCollectibleTypeAlive()
    {
        var typeReference = CacheCollectibleType();

        for (var attempt = 0; typeReference.IsAlive && attempt < 10; attempt++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
        }

        Assert.False(typeReference.IsAlive);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CacheCollectibleType()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName("FclEx.CollectibleTypeInfoExTest." + Guid.NewGuid()),
            AssemblyBuilderAccess.RunAndCollect);
        var type = assembly.DefineDynamicModule("Main")
            .DefineType("CollectibleType", TypeAttributes.Public)
            .CreateTypeInfo()!
            .AsType();

        _ = type.GetTypeInfoEx();
        return new WeakReference(type);
    }
}
