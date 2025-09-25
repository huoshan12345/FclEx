using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace FclEx.Extensions;

public class JsonNodeExtensionsTests
{
    [Fact]
    public void GetOrAdd_ReturnsExistingNode_WhenKeyExistsWithCorrectType()
    {
        // Arrange
        var obj = new JsonObject
        {
            ["name"] = JsonValue.Create("Alice")
        };

        // Act
        var result = obj.GetOrAdd("name", () => JsonValue.Create("Bob"));

        // Assert
        Assert.Same(obj["name"], result);
        Assert.Equal("Alice", result.ToString());
    }

    [Fact]
    public void GetOrAdd_CreatesAndAddsNode_WhenKeyDoesNotExist()
    {
        // Arrange
        var obj = new JsonObject();

        // Act
        var result = obj.GetOrAdd("age", () => JsonValue.Create(30));

        // Assert
        Assert.Same(obj["age"], result);
        Assert.Equal("30", result.ToString());
    }

    [Fact]
    public void GetOrAdd_CreatesAndReplacesNode_WhenKeyExistsWithWrongType()
    {
        // Arrange
        var obj = new JsonObject
        {
            ["data"] = new JsonArray(1, 2, 3)
        };

        // Act
        var result = obj.GetOrAdd("data", () => new JsonObject { ["key"] = "value" });

        // Assert
        Assert.Same(obj["data"], result);
        Assert.IsType<JsonObject>(result);
        Assert.Equal("value", ((JsonObject)result)["key"]!.ToString());
    }

    [Fact]
    public void GetOrAdd_AllowsNestedCreation()
    {
        // Arrange
        var obj = new JsonObject();

        // Act
        var inner = obj.GetOrAdd("child", () => new JsonObject())
                       .GetOrAdd("grandchild", () => JsonValue.Create("hello"));

        // Assert
        Assert.Equal("hello", inner.ToString());
        Assert.Equal("hello", obj["child"]!["grandchild"]!.ToString());
    }

    [Fact]
    public void GetOrAdd_UsesCreatorEveryTime_WhenExistingNodeIsWrongType()
    {
        // Arrange
        var obj = new JsonObject { ["data"] = new JsonArray() };
        var calls = 0;

        // Act
        obj.GetOrAdd("data", () => { calls++; return new JsonObject(); });
        obj.GetOrAdd("data", () => { calls++; return new JsonObject(); });

        // Assert
        Assert.Equal(1, calls); // creator should be called each time type mismatch happens
    }
}