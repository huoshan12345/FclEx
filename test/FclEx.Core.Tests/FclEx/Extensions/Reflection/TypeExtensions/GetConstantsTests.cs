namespace FclEx.Extensions.Reflection.TypeExtensions;

public class GetConstantsTests
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TestClass
    {
        public const int MyConst = 10;
        public readonly int MyReadonlyField = 20;
        public static readonly int MyStaticReadonlyField = 30;
        public int MyField = 40;
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class NoConstantsClass
    {
        public readonly int ReadonlyField = 10;
        public int NonConstantField = 20;
    }

    [Fact]
    public void GetConstants_ShouldReturnOnlyConstFields()
    {
        var result = typeof(TestClass).GetConstants();

        Assert.Single(result);
        Assert.Equal("MyConst", result[0].Name);
        Assert.Equal(typeof(int), result[0].FieldType);
        Assert.True(result[0].IsLiteral);
        Assert.False(result[0].IsInitOnly);
    }

    [Fact]
    public void GetConstants_ShouldReturnEmpty_WhenNoConstFieldsExist()
    {
        var result = typeof(NoConstantsClass).GetConstants();

        Assert.Empty(result);
    }
}
