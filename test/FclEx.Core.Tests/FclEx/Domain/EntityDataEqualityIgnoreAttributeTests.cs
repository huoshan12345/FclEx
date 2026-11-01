namespace FclEx.Domain;

[Collection("EntityDataEquality")]
public class EntityDataEqualityIgnoreAttributeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IgnoredFieldsAndProperties_DoNotAffectEqualityOrHashCode(bool useEmptyExclusions)
    {
        var comparer = EntityDataEqualityComparer.Create<IgnoredModel>(useEmptyExclusions ? [] : null);
        var first = new IgnoredModel { IgnoredField = 1, Items = [1] };
        var second = new IgnoredModel { IgnoredField = 2, Items = [2] };
        Assert.True(comparer.Equals(first, second));
        Assert.Equal(comparer.GetHashCode(first), comparer.GetHashCode(second));
    }

    [Fact]
    public void InheritedProperty_RemainsIgnored()
    {
        var comparer = EntityDataEqualityComparer<InheritedModel>.Instance;
        var first = new InheritedModel { Value = 1 };
        var second = new InheritedModel { Value = 2 };
        Assert.True(comparer.Equals(first, second));
        Assert.Equal(comparer.GetHashCode(first), comparer.GetHashCode(second));
    }

    [Fact]
    public void OverrideWithoutAttribute_ParticipatesInComparison()
    {
        var comparer = EntityDataEqualityComparer<OverrideModel>.Instance;
        Assert.False(comparer.Equals(new OverrideModel { Value = 1 }, new OverrideModel { Value = 2 }));
    }

    [Fact]
    public void OverrideWithAttribute_IsIgnored()
    {
        var comparer = EntityDataEqualityComparer<MarkedOverrideModel>.Instance;
        var first = new MarkedOverrideModel { Value = 1 };
        var second = new MarkedOverrideModel { Value = 2 };
        Assert.True(comparer.Equals(first, second));
        Assert.Equal(comparer.GetHashCode(first), comparer.GetHashCode(second));
    }

    public class IgnoredModel
    {
        [EntityDataEqualityIgnore]
        public int IgnoredField;

        [EntityDataEqualityIgnore]
        public List<int>? Items { get; set; }

        [EntityDataEqualityIgnore]
        public int ThrowingProperty => throw new InvalidOperationException("Ignored getters must not be called.");
    }

    public class BaseModel
    {
        [EntityDataEqualityIgnore]
        public virtual int Value { get; set; }
    }

    public class InheritedModel : BaseModel;

    public class OverrideModel : BaseModel
    {
        public override int Value { get; set; }
    }

    public class MarkedOverrideModel : BaseModel
    {
        [EntityDataEqualityIgnore]
        public override int Value { get; set; }
    }
}
