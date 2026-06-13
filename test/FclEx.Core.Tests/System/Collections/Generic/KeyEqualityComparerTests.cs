namespace System.Collections.Generic;

public class KeyEqualityComparerTests
{
    private sealed class Person
    {
        public string? Name { get; init; }
    }

    [Fact]
    public void Equals_ShouldUseSelectedKey()
    {
        var comparer = KeyEqualityComparer<Person>.Create(x => x.Name);

        Assert.True(comparer.Equals(new Person { Name = "Alice" }, new Person { Name = "Alice" }));
        Assert.False(comparer.Equals(new Person { Name = "Alice" }, new Person { Name = "Bob" }));
    }

    [Fact]
    public void Equals_ShouldUseCustomKeyComparer()
    {
        var comparer = KeyEqualityComparer<Person>.Create(x => x.Name, StringComparer.OrdinalIgnoreCase);

        Assert.True(comparer.Equals(new Person { Name = "Alice" }, new Person { Name = "alice" }));
        Assert.Equal(
            comparer.GetHashCode(new Person { Name = "Alice" }),
            comparer.GetHashCode(new Person { Name = "alice" }));
    }

    [Fact]
    public void GetHashCode_ShouldReturnZero_ForNullKey()
    {
        var comparer = KeyEqualityComparer<Person>.Create(x => x.Name);

        Assert.Equal(0, comparer.GetHashCode(new Person { Name = null }));
    }

    [Fact]
    public void Equals_ShouldHandleNullObjects()
    {
        var comparer = KeyEqualityComparer<Person>.Create(x => x.Name);
        var person = new Person { Name = "Alice" };

        Assert.True(comparer.Equals(null, null));
        Assert.False(comparer.Equals(null, person));
        Assert.False(comparer.Equals(person, null));
    }
}
