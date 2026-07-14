#pragma warning disable CS0169
#pragma warning disable CS0649

namespace FclEx.Extensions.Reflection.TypeExtensions;

public class GetMemberTests
{
    private class BaseModel
    {
        public int BaseField;
        public int BaseProperty { get; set; }

        public void BaseMethod() { }
    }

    private class Model : BaseModel
    {
        public int Field;
        public int AutoProperty { get; set; }
        public int Property { get; set; }

        public Model() { }

        private Model(int value)
        {
            Field = value;
        }

        public void Method() { }

        public void MethodWithParameter(string value) { }

        public T GenericMethod<T>(string value) => default!;
    }

    [Fact]
    public void GetRequiredField_ShouldReturnDeclaredField()
    {
        var field = typeof(Model).GetRequiredField(nameof(Model.Field));

        Assert.Equal(nameof(Model.Field), field.Name);
    }

    [Fact]
    public void GetRequiredField_ShouldThrow_WhenFieldIsMissing()
    {
        Assert.Throws<InvalidOperationException>(() => typeof(Model).GetRequiredField("Missing"));
    }

    [Fact]
    public void GetRequiredField_ShouldSearchBaseTypes_WhenRequested()
    {
        var field = typeof(Model).GetRequiredField(nameof(BaseModel.BaseField), searchBaseTypes: true);

        Assert.Equal(nameof(BaseModel.BaseField), field.Name);
    }

    [Fact]
    public void GetProperty_ShouldReturnNull_WhenPropertyIsMissing()
    {
        Assert.Null(typeof(Model).GetProperty("Missing", searchBaseTypes: true));
    }

    [Fact]
    public void GetRequiredProperty_ShouldReturnDeclaredProperty()
    {
        var property = typeof(Model).GetRequiredProperty(nameof(Model.Property));

        Assert.Equal(nameof(Model.Property), property.Name);
    }

    [Fact]
    public void GetRequiredProperty_ShouldSearchBaseTypes_WhenRequested()
    {
        var property = typeof(Model).GetRequiredProperty(nameof(BaseModel.BaseProperty), searchBaseTypes: true);

        Assert.Equal(nameof(BaseModel.BaseProperty), property.Name);
    }

    [Fact]
    public void GetRequiredProperty_ShouldThrow_WhenPropertyIsMissing()
    {
        Assert.Throws<InvalidOperationException>(() => typeof(Model).GetRequiredProperty("Missing"));
    }

    [Fact]
    public void GetAutoPropertyBackingField_ShouldReturnBackingField()
    {
        var field = typeof(Model).GetAutoPropertyBackingField(nameof(Model.AutoProperty));

        Assert.Equal("<AutoProperty>k__BackingField", field.Name);
    }

    [Fact]
    public void GetAutoPropertyOrFieldName_ShouldReturnPropertyName_ForAutoPropertyBackingField()
    {
        var field = typeof(Model).GetAutoPropertyBackingField(nameof(Model.AutoProperty));

        Assert.Equal(nameof(Model.AutoProperty), field.GetAutoPropertyOrFieldName());
    }

    [Fact]
    public void GetAutoPropertyOrFieldName_ShouldReturnFieldName_ForNormalField()
    {
        var field = typeof(Model).GetRequiredField(nameof(Model.Field));

        Assert.Equal(nameof(Model.Field), field.GetAutoPropertyOrFieldName());
    }

    [Fact]
    public void GetMethod_ShouldReturnNull_WhenMethodIsMissing()
    {
        Assert.Null(typeof(Model).GetMethod("Missing", searchBaseTypes: true));
    }

    [Fact]
    public void GetRequiredMethod_ShouldReturnDeclaredMethod()
    {
        var method = typeof(Model).GetRequiredMethod(nameof(Model.Method));

        Assert.Equal(nameof(Model.Method), method.Name);
    }

    [Fact]
    public void GetRequiredMethod_ShouldSearchBaseTypes_WhenRequested()
    {
        var method = typeof(Model).GetRequiredMethod(nameof(BaseModel.BaseMethod), searchBaseTypes: true);

        Assert.Equal(nameof(BaseModel.BaseMethod), method.Name);
    }

    [Fact]
    public void GetRequiredMethod_ShouldThrow_WhenMethodIsMissing()
    {
        Assert.Throws<InvalidOperationException>(() => typeof(Model).GetRequiredMethod("Missing"));
    }

    [Fact]
    public void GetMethod_WithSignature_ShouldReturnMatchingMethod()
    {
        var method = typeof(Model).GetMethod(nameof(Model.MethodWithParameter), 0, [typeof(string)]);

        Assert.NotNull(method);
        Assert.Equal(nameof(Model.MethodWithParameter), method.Name);
    }

    [Fact]
    public void GetMethod_WithGenericArgumentCount_ShouldReturnMatchingGenericMethod()
    {
        var method = typeof(Model).GetMethod(nameof(Model.GenericMethod), 1, [typeof(string)]);

        Assert.NotNull(method);
        Assert.Equal(nameof(Model.GenericMethod), method.Name);
    }

    [Fact]
    public void GetRequiredMethod_WithSignature_ShouldThrow_WhenMethodIsMissing()
    {
        Assert.Throws<InvalidOperationException>(() => typeof(Model).GetRequiredMethod("Missing", 0, []));
    }

    [Fact]
    public void GetRequiredConstructor_ShouldReturnNonPublicConstructor()
    {
        var ctor = typeof(Model).GetRequiredConstructor(typeof(int));

        Assert.True(ctor.IsPrivate);
    }

    [Fact]
    public void GetRequiredConstructor_ShouldThrow_WhenConstructorIsMissing()
    {
        Assert.Throws<InvalidOperationException>(() => typeof(Model).GetRequiredConstructor(typeof(string)));
    }

    [Fact]
    public void GetDataMember_ShouldReturnNull_WhenMemberIsMissing()
    {
        Assert.Null(typeof(Model).GetDataMember("Missing"));
    }

    [Fact]
    public void GetDataMember_ShouldReturnMatchingFieldOrProperty()
    {
        var member = typeof(Model).GetDataMember(nameof(Model.Property));

        Assert.NotNull(member);
        Assert.Equal(nameof(Model.Property), member.Name);
    }

    [Fact]
    public void GetRequiredDataMember_ShouldThrow_WhenMemberIsMissing()
    {
        Assert.Throws<InvalidOperationException>(() => typeof(Model).GetRequiredDataMember("Missing"));
    }
}
