namespace FclEx.EfCore.Extensions;

partial class DbContextExtensionsTests
{
    [Theory]
    [InlineData("TestEntity")]
    [InlineData("TestEntities")]
    public void TestingHelpers_ShouldNotBePublicPackageApi(string methodName)
    {
        var publicMethods = typeof(DbContextExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static);

        Assert.DoesNotContain(publicMethods, method => method.Name == methodName);
    }
}
