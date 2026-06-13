namespace System.Collections.Generic;

public class DelegateEqualityComparerTests
{
    private sealed class Person
    {
        public int Id { get; init; }
    }

    [Fact]
    public void Equals_ShouldUseDelegate_ForNonNullValues()
    {
        var comparer = DelegateEqualityComparer.Create<Person>(
            (x, y) => x.Id == y.Id,
            x => x.Id);

        Assert.True(comparer.Equals(new Person { Id = 1 }, new Person { Id = 1 }));
        Assert.False(comparer.Equals(new Person { Id = 1 }, new Person { Id = 2 }));
    }

    [Fact]
    public void Equals_ShouldHandleNullValues_BeforeCallingDelegate()
    {
        var comparer = DelegateEqualityComparer.Create<Person>(
            (_, _) => throw new InvalidOperationException("The delegate should not be called for null operands."),
            x => x.Id);

        var person = new Person { Id = 1 };

        Assert.True(comparer.Equals(null, null));
        Assert.False(comparer.Equals(null, person));
        Assert.False(comparer.Equals(person, null));
    }

    [Fact]
    public void GetHashCode_ShouldUseDelegate()
    {
        var comparer = DelegateEqualityComparer.Create<Person>(
            (x, y) => x.Id == y.Id,
            x => x.Id);

        Assert.Equal(42, comparer.GetHashCode(new Person { Id = 42 }));
    }
}
