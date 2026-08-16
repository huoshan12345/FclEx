namespace FclEx.Extensions.Reflection.TypeExtensions;

public class BasicTests
{
    public struct StructWithParameterlessConstructor
    {
        public StructWithParameterlessConstructor()
        {
            Value = 42;
        }

        public int Value { get; }
    }

    public interface ISample;

    public interface IGenericSample<T>;

    public class SampleImplementation : ISample;

    public class GenericSampleImplementation : IGenericSample<int>;

    public class Base;

    public class Derived : Base;

    public class GenericBase<T>;

    public class GenericDerived : GenericBase<int>;

    public class SourceDefinedConversion
    {
        public static implicit operator ConversionTarget(SourceDefinedConversion value) => new();
    }

    public class TargetDefinedConversion
    {
        public static implicit operator TargetDefinedConversion(SourceForTargetDefinedConversion value) => new();
    }

    public class SourceForTargetDefinedConversion;

    public class ConversionTarget;

    public class ExplicitConversionSource
    {
        public static explicit operator ExplicitConversionTarget(ExplicitConversionSource value) => new();
    }

    public class ExplicitConversionTarget;

    [Fact]
    public void GetImplementedInterface_ShouldNotReturnTypeItself()
    {
        Assert.Null(typeof(ISample).GetImplementedInterface(typeof(ISample)));
        Assert.False(typeof(ISample).Implements(typeof(ISample)));
    }

    [Fact]
    public void GetImplementedInterface_ShouldReturnImplementedInterface()
    {
        var result = typeof(SampleImplementation).GetImplementedInterface(typeof(ISample));

        Assert.Equal(typeof(ISample), result);
        Assert.True(typeof(SampleImplementation).Implements(typeof(ISample)));
    }

    [Fact]
    public void GetImplementedInterface_ShouldMatchGenericInterfaceDefinition()
    {
        var result = typeof(GenericSampleImplementation).GetImplementedInterface(typeof(IGenericSample<>));

        Assert.Equal(typeof(IGenericSample<int>), result);
        Assert.True(typeof(GenericSampleImplementation).Implements(typeof(IGenericSample<>)));
    }

    [Fact]
    public void Inherits_ShouldNotIncludeTypeItself()
    {
        Assert.False(typeof(Base).Inherits(typeof(Base)));
    }

    [Fact]
    public void Inherits_ShouldReturnTrueForBaseType()
    {
        Assert.True(typeof(Derived).Inherits(typeof(Base)));
    }

    [Fact]
    public void Inherits_ShouldMatchGenericBaseTypeDefinition()
    {
        Assert.True(typeof(GenericDerived).Inherits(typeof(GenericBase<>)));
    }

    [Fact]
    public void HasImplicitConversionOperator_ShouldFindOperatorDeclaredOnSourceType()
    {
        Assert.True(typeof(SourceDefinedConversion).HasImplicitConversionOperator(typeof(ConversionTarget)));
    }

    [Fact]
    public void HasImplicitConversionOperator_ShouldFindOperatorDeclaredOnTargetType()
    {
        Assert.True(typeof(SourceForTargetDefinedConversion).HasImplicitConversionOperator(typeof(TargetDefinedConversion), typeof(TargetDefinedConversion)));
    }

    [Fact]
    public void HasImplicitConversionOperator_ShouldSearchSourceAndTargetTypes_WhenDeclaringTypeIsNotSpecified()
    {
        Assert.True(typeof(SourceForTargetDefinedConversion).HasImplicitConversionOperator(typeof(TargetDefinedConversion)));
    }

    [Fact]
    public void HasImplicitConversionOperator_ShouldReturnFalseForExplicitOperator()
    {
        Assert.False(typeof(ExplicitConversionSource).HasImplicitConversionOperator(typeof(ExplicitConversionTarget)));
    }

    [Fact]
    public void IsNullable_ShouldReturnTrueOnlyForNullableValueType()
    {
        Assert.True(typeof(int?).IsNullable());
        Assert.False(typeof(string).IsNullable());
        Assert.False(typeof(int).IsNullable());
    }

    [Fact]
    public void NullableUnderlyingType_ShouldReturnUnderlyingValueType()
    {
        Assert.Equal(typeof(int), typeof(int?).NullableUnderlyingType());
        Assert.Null(typeof(int).NullableUnderlyingType());
    }

    [Fact]
    public void DefaultValue_ShouldNotCallStructParameterlessConstructor()
    {
        var value = Assert.IsType<StructWithParameterlessConstructor>(typeof(StructWithParameterlessConstructor).DefaultValue());

        Assert.Equal(0, value.Value);
    }

    [Fact]
    public void IsInteger_ShouldIncludeNativeSizedIntegers()
    {
        Assert.True(typeof(nint).IsInteger());
        Assert.True(typeof(nuint).IsInteger());
    }

    [Theory]
    [InlineData(typeof(float), true)]
    [InlineData(typeof(double), true)]
    [InlineData(typeof(decimal), false)]
    [InlineData(typeof(decimal?), false)]
    [InlineData(typeof(int), false)]
#if NET5_0_OR_GREATER
    [InlineData(typeof(Half), true)]
    [InlineData(typeof(Half?), true)]
#endif
    public void IsFloatingPoint_ShouldReturnExpectedResult(Type type, bool expected)
    {
        Assert.Equal(expected, type.IsFloatingPoint());
    }

    [Theory]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(int?), true)]
    [InlineData(typeof(decimal), true)]
    [InlineData(typeof(decimal?), true)]
    [InlineData(typeof(BigInteger), true)]
    [InlineData(typeof(BigInteger?), true)]
    [InlineData(typeof(string), false)]
    public void IsNumeric_ShouldReturnExpectedResult(Type type, bool expected)
    {
        Assert.Equal(expected, type.IsNumeric());
    }
}
