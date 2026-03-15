namespace FclEx.Xunit;

partial class XunitSerializableAttributeTests
{
    [XunitSerializable]
    public partial record TestModel<T>(Type? Type, Expression<Func<T, object?>>? Selector, object? Value, bool Valid = false)
    {
        public string Member { get; } = Selector is null ? "" : ExpressionHelper.GetDataMemberInfo(Selector).Name;

        public override string ToString() => $"{Member} -> {Value?.ToString().IfEmpty("\"\"") ?? "null"} {(Valid ? "√" : "×")}";
    }

    [Fact]
    public void Should_RoundTrip_With_Type_Expression()
    {
        var original = new TestModel<string>(typeof(string), m => m.Length, 1);
        var info = CreateSerializationInfo();
        original.Serialize(info);

        var deserialized = new TestModel<string>(null, null, null);
        deserialized.Deserialize(info);

        Assert.Equal(original, deserialized);
    }
}
