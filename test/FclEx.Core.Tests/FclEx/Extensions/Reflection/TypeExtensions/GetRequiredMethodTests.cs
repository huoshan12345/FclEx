namespace FclEx.Extensions.Reflection.TypeExtensions;

public class GetRequiredMethodTests
{
    private class BaseModel
    {
        private static void BaseMethod(string value) { }

        public void Overloaded(int value) { }
    }

    private class Model : BaseModel
    {
        public void Parameterless() { }

        public void WithParameter(string value) { }

        public void Generic<T>(string value) { }

        public void Overloaded() { }

        public void Overloaded(string value) { }
    }

    public static TheoryData<string, int, Type[]?, bool, bool> ParameterMatchingCases => new()
    {
        { nameof(Model.Parameterless), 0, null, false, true },
        { nameof(Model.Parameterless), 0, [], false, true },
        { nameof(Model.WithParameter), 0, null, false, true },
        { nameof(Model.WithParameter), 0, [], false, false },
        { nameof(Model.WithParameter), 0, [typeof(string)], false, true },
        { nameof(Model.WithParameter), 0, [typeof(int)], false, false },
        { nameof(Model.WithParameter), 0, [typeof(string), typeof(string)], false, false },
        { nameof(Model.Generic), 1, null, false, true },
        { nameof(Model.Generic), 0, null, false, false },
        { nameof(Model.Generic), 2, null, false, false },
        { nameof(Model.Generic), 1, [], false, false },
        { nameof(Model.Generic), 1, [typeof(string)], false, true },
        { "BaseMethod", 0, null, false, false },
        { "BaseMethod", 0, null, true, true },
        { "BaseMethod", 0, [], true, false },
        { "BaseMethod", 0, [typeof(string)], true, true },
        { "Missing", 0, null, true, false },
    };

    [Theory]
    [MemberData(nameof(ParameterMatchingCases))]
    public void GetRequiredMethod_ShouldMatchParametersOnlyWhenSpecified(
        string name, int genericArgumentCount, Type[]? paramTypes, bool searchBaseTypes, bool shouldMatch)
    {
        if (shouldMatch)
        {
            var method = typeof(Model).GetRequiredMethod(name, genericArgumentCount, paramTypes, searchBaseTypes);

            Assert.Equal(name, method.Name);
            Assert.Equal(genericArgumentCount, method.GetGenericArguments().Length);
            if (paramTypes is not null)
                Assert.Equal(paramTypes, method.GetParameters().Select(p => p.ParameterType));
        }
        else
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                typeof(Model).GetRequiredMethod(name, genericArgumentCount, paramTypes, searchBaseTypes));

            Assert.Contains(name, exception.Message);
            Assert.Contains(typeof(Model).FullName!, exception.Message);
        }
    }

    [Theory]
    [InlineData(nameof(Model.Parameterless), 0)]
    [InlineData(nameof(Model.WithParameter), 0)]
    [InlineData(nameof(Model.Generic), 1)]
    public void GetRequiredMethod_ShouldIgnoreParameters_WhenParameterTypesAreOmitted(string name, int genericArgumentCount)
    {
        var method = typeof(Model).GetRequiredMethod(name, genericArgumentCount);

        Assert.Equal(name, method.Name);
        Assert.Equal(genericArgumentCount, method.GetGenericArguments().Length);
        Assert.Equal(typeof(Model), method.DeclaringType);
    }

    [Theory]
    [InlineData("Missing", 0)]
    [InlineData(nameof(Model.Generic), 0)]
    [InlineData("BaseMethod", 0)]
    public void GetRequiredMethod_WithoutParameterTypes_ShouldThrow_WhenNoDeclaredMethodMatches(string name, int genericArgumentCount)
    {
        Assert.Throws<InvalidOperationException>(() => typeof(Model).GetRequiredMethod(name, genericArgumentCount));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetRequiredMethod_WithoutParameterTypes_ShouldSearchBaseTypesOnlyWhenRequested(bool searchBaseTypes)
    {
        if (searchBaseTypes)
        {
            var method = typeof(Model).GetRequiredMethod("BaseMethod", 0, searchBaseTypes);

            Assert.Equal(typeof(BaseModel), method.DeclaringType);
            Assert.True(method.IsPrivate);
            Assert.True(method.IsStatic);
            Assert.Equal(typeof(string), Assert.Single(method.GetParameters()).ParameterType);
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => typeof(Model).GetRequiredMethod("BaseMethod", 0, searchBaseTypes));
        }
    }

    [Fact]
    public void GetRequiredMethod_ShouldMatchOnlyParameterlessMethods_WhenEmptyArrayIsExplicit()
    {
        var method = typeof(Model).GetRequiredMethod(nameof(Model.Parameterless), 0, []);

        Assert.Empty(method.GetParameters());
        Assert.Throws<InvalidOperationException>(() => typeof(Model).GetRequiredMethod(nameof(Model.WithParameter), 0, []));
    }

    [Fact]
    public void GetRequiredMethod_ShouldStillMatchExactTypes_WithParamsArguments()
    {
        var method = typeof(Model).GetRequiredMethod(nameof(Model.WithParameter), 0, typeof(string));

        Assert.Equal(typeof(string), Assert.Single(method.GetParameters()).ParameterType);
    }

    [Fact]
    public void GetRequiredMethod_ShouldReturnFirstDeclaredMatch_BeforeSearchingBaseTypes()
    {
        var expected = typeof(Model).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .First(m => m.Name == nameof(Model.Overloaded));

        var withNullParameters = typeof(Model).GetRequiredMethod(nameof(Model.Overloaded), 0, null, true);
        var withoutParameterTypes = typeof(Model).GetRequiredMethod(nameof(Model.Overloaded), 0, searchBaseTypes: true);

        Assert.Equal(expected, withNullParameters);
        Assert.Equal(expected, withoutParameterTypes);
    }
}
