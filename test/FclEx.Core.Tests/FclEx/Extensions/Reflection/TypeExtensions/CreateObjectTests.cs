namespace FclEx.Extensions.Reflection.TypeExtensions;

public class CreateObjectTests
{
    public class DefaultParameterCtor
    {
        public DefaultParameterCtor(int id, string name = "default", int? count = 7)
        {
            Id = id;
            Name = name;
            Count = count;
        }

        public int Id { get; }
        public string? Name { get; }
        public int? Count { get; }
    }

    public class AllDefaultParameterCtor
    {
        public AllDefaultParameterCtor(string name = "default", int? count = 7)
        {
            Name = name;
            Count = count;
        }

        public string? Name { get; }
        public int? Count { get; }
    }

    public class NullableCtor
    {
        public NullableCtor(int? value)
        {
            Value = value;
        }

        public int? Value { get; }
    }

    public class ReferenceCtor
    {
        public ReferenceCtor(string? value)
        {
            Value = value;
        }

        public string? Value { get; }
    }

    public class NonNullableValueCtor
    {
        public NonNullableValueCtor(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }

    public class PrimitiveWideningCtor
    {
        public PrimitiveWideningCtor(long value)
        {
            Value = value;
        }

        public long Value { get; }
    }

    public class AmbiguousNullCtor
    {
        public AmbiguousNullCtor(string? value)
        {
            Value = value;
        }

        public AmbiguousNullCtor(Uri? value)
        {
            Value = value;
        }

        public object? Value { get; }
    }

    public class AmbiguousDefaultParameterCtor
    {
        public AmbiguousDefaultParameterCtor(int id)
        {
            Id = id;
        }

        public AmbiguousDefaultParameterCtor(int id, string name = "default")
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string? Name { get; }
    }

    public class AmbiguousParameterlessAndDefaultParameterCtor
    {
        public AmbiguousParameterlessAndDefaultParameterCtor()
        {
        }

        public AmbiguousParameterlessAndDefaultParameterCtor(string name = "default")
        {
            Name = name;
        }

        public string? Name { get; }
    }

    [Fact]
    public void CreateObject_ShouldUseDefaultParameterValues_WhenArgumentsAreOmitted()
    {
        var obj = typeof(DefaultParameterCtor).CreateObject<DefaultParameterCtor>(1);

        Assert.Equal(1, obj.Id);
        Assert.Equal("default", obj.Name);
        Assert.Equal(7, obj.Count);
    }

    [Fact]
    public void CreateObject_ShouldUseDefaultParameterValues_WhenAllArgumentsAreOmitted()
    {
        var obj = typeof(AllDefaultParameterCtor).CreateObject<AllDefaultParameterCtor>();

        Assert.Equal("default", obj.Name);
        Assert.Equal(7, obj.Count);
    }

    [Fact]
    public void CreateObject_TreatsANullParamsArrayAsNoArguments()
    {
        var obj = typeof(AllDefaultParameterCtor).CreateObject<AllDefaultParameterCtor>((object?[]?)null!);

        Assert.Equal("default", obj.Name);
        Assert.Equal(7, obj.Count);
    }

    [Fact]
    public void CreateObject_ShouldMatchNull_ToReferenceType()
    {
        var obj = typeof(ReferenceCtor).CreateObject<ReferenceCtor>((object?)null);

        Assert.Null(obj.Value);
    }

    [Fact]
    public void CreateObject_ShouldMatchNull_ToNullableValueType()
    {
        var obj = typeof(NullableCtor).CreateObject<NullableCtor>((object?)null);

        Assert.Null(obj.Value);
    }

    [Fact]
    public void CreateObject_ShouldNotMatchNull_ToNonNullableValueType()
    {
        Assert.Throws<MissingMethodException>(() => typeof(NonNullableValueCtor).CreateObject((object?)null));
    }

    [Fact]
    public void CreateObject_ShouldMatchPrimitiveWideningConversion()
    {
        var obj = typeof(PrimitiveWideningCtor).CreateObject<PrimitiveWideningCtor>(1);

        Assert.Equal(1L, obj.Value);
    }

    [Fact]
    public void CreateObject_ShouldThrowAmbiguousMatchException_WhenNullMatchesMultipleConstructors()
    {
        Assert.Throws<AmbiguousMatchException>(() => typeof(AmbiguousNullCtor).CreateObject((object?)null));
    }

    [Fact]
    public void CreateObject_ShouldThrowAmbiguousMatchException_WhenDefaultParametersMatchMultipleConstructors()
    {
        Assert.Throws<AmbiguousMatchException>(() => typeof(AmbiguousDefaultParameterCtor).CreateObject(1));
    }

    [Fact]
    public void CreateObject_ShouldThrowAmbiguousMatchException_WhenParameterlessAndDefaultParameterConstructorsMatch()
    {
        Assert.Throws<AmbiguousMatchException>(() => typeof(AmbiguousParameterlessAndDefaultParameterCtor).CreateObject());
    }
}
