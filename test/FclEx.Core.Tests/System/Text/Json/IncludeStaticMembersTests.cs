using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace System.Text.Json;

public class IncludeStaticMembersTests
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions().AddModifierForStaticMembers();

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TestModel
    {
        [JsonInclude]
        public static readonly string StaticIncludedField = "Included Field";

        public static readonly string StaticExcludedField = "Excluded Field";

        [JsonInclude]
        public static string StaticIncludedProperty { get; } = "Included Property";

        public static string StaticExcludedProperty { get; } = "Excluded Property";

        [JsonInclude]
        public const string ConstIncludedField = "Included Const Field";

        public const string ConstExcludedField = "Excluded Const Field";

        [JsonInclude]
        public static object MutableIncludedProperty { get; set; } = "Initial Value";
    }

    [Fact]
    public void AddStaticMembers_ShouldIncludeStaticMembersWithJsonIncludeAttribute()
    {
        var json = JsonSerializer.Serialize(new TestModel(), _options);

        Assert.Contains("\"StaticIncludedField\":\"Included Field\"", json);
        Assert.Contains("\"StaticIncludedProperty\":\"Included Property\"", json);
        Assert.Contains("\"ConstIncludedField\":\"Included Const Field\"", json);

        Assert.DoesNotContain("\"StaticExcludedField\"", json);
        Assert.DoesNotContain("\"StaticExcludedProperty\"", json);
        Assert.DoesNotContain("\"ConstExcludedField\"", json);
    }

    [Fact]
    public void AddStaticMembers_ShouldNotIncludeStaticMembersWithoutModifier()
    {
        var options = new JsonSerializerOptions();
        var json = JsonSerializer.Serialize(new TestModel(), options);

        Assert.DoesNotContain("\"StaticIncludedField\"", json);
        Assert.DoesNotContain("\"StaticIncludedProperty\"", json);
        Assert.DoesNotContain("\"StaticExcludedField\"", json);
        Assert.DoesNotContain("\"StaticExcludedProperty\"", json);
    }

    [Fact]
    public void AddStaticMembers_Reads_The_Current_Value_Using_The_Declared_Member_Type()
    {
        try
        {
            TestModel.MutableIncludedProperty = "Initial Value";
            var initialJson = JsonSerializer.Serialize(new TestModel(), _options);

            TestModel.MutableIncludedProperty = 42;
            var updatedJson = JsonSerializer.Serialize(new TestModel(), _options);

            Assert.Contains("\"MutableIncludedProperty\":\"Initial Value\"", initialJson);
            Assert.Contains("\"MutableIncludedProperty\":42", updatedJson);
        }
        finally
        {
            TestModel.MutableIncludedProperty = "Initial Value";
        }
    }
}
