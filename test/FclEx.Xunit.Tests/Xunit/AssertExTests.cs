namespace Xunit;

public partial class AssertExTests
{
    public enum Flags
    {
        None = 0,
        False,
        True,
    }

    public static readonly IEnumerable<object?[]> EqualTestCases = Enum.GetValues(typeof(Flags))
        .Cast<Flags?>()
        .Concat(new Flags?[] { null })
        .Select(m => new object?[] { m, m.CastTo<int?>() });

    [Theory]
    [MemberData(nameof(EqualTestCases))]
    public void Equal_Enum_Int_Test(Flags? flags, int? number)
    {
        AssertEx.Equal(flags, number);
    }

    [Fact]
    public void Equal_Enum_Int_Failed()
    {
        Assert.Throws<EqualException>(() => AssertEx.Equal(Flags.False, 0));
        Assert.Throws<EqualException>(() => AssertEx.Equal((Flags?)null, (int?)0));
    }
}