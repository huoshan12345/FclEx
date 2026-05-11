namespace System.Collections.Generic;

public partial class MemberEqualityComparerBuilderTests
{
    private sealed class Person
    {
        public int Id { get; init; }
        public string? Name { get; init; }
        public int Age { get; init; }
    }

    private readonly struct OrderKey
    {
        public int StoreId { get; init; }
        public long OrderId { get; init; }
    }

    [Fact]
    public void Same_Member_Values_Should_Be_Equal()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(x => x.Id)
            .Build();

        var a = new Person { Id = 1 };
        var b = new Person { Id = 1 };

        Assert.True(cmp.Equals(a, b));
    }

    [Fact]
    public void Different_Member_Values_Should_Not_Be_Equal()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(x => x.Id)
            .Build();

        var a = new Person { Id = 1 };
        var b = new Person { Id = 2 };

        Assert.False(cmp.Equals(a, b));
    }

    [Fact]
    public void Equal_Objects_Should_Have_Same_Hashcode()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(x => x.Id)
            .Add(x => x.Age)
            .Build();

        var a = new Person { Id = 1, Age = 18 };
        var b = new Person { Id = 1, Age = 18 };

        Assert.Equal(
            cmp.GetHashCode(a),
            cmp.GetHashCode(b));
    }

    [Fact]
    public void Null_Should_Equal_Null()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(x => x.Id)
            .Build();

        Assert.True(cmp.Equals(null!, null!));
    }

    [Fact]
    public void Null_Should_Not_Equal_Instance()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(x => x.Id)
            .Build();

        var a = new Person { Id = 1 };

        Assert.False(cmp.Equals(a, null!));
    }

    [Fact]
    public void Should_Use_Custom_Member_Comparer()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Build();

        var a = new Person { Name = "Alice" };
        var b = new Person { Name = "alice" };

        Assert.True(cmp.Equals(a, b));
    }

    [Fact]
    public void Should_Compare_All_Members()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(x => x.Id)
            .Add(x => x.Age)
            .Build();

        var a = new Person { Id = 1, Age = 18 };
        var b = new Person { Id = 1, Age = 19 };

        Assert.False(cmp.Equals(a, b));
    }

    [Fact]
    public void Should_Work_Inside_Dictionary_Key_Lookup()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(x => x.Id)
            .Build();

        var dict = new Dictionary<Person, string>(cmp);

        var key = new Person { Id = 1 };
        dict[key] = "ok";

        var lookup = new Person { Id = 1 };

        Assert.True(dict.ContainsKey(lookup));
    }

    [Fact]
    public void Should_Work_Inside_Hashset()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(x => x.Id)
            .Build();

        var set = new HashSet<Person>(cmp)
        {
            new (){ Id = 1 }
        };

        Assert.Contains(new Person { Id = 1 }, set);
    }

    [Fact]
    public void Should_Support_Struct_Members_Without_Breaking_Equality()
    {
        var cmp = MemberEqualityComparerBuilder<OrderKey>
            .Create()
            .Add(x => x.StoreId)
            .Add(x => x.OrderId)
            .Build();

        var a = new OrderKey { StoreId = 1, OrderId = 99 };
        var b = new OrderKey { StoreId = 1, OrderId = 99 };

        Assert.True(cmp.Equals(a, b));
        Assert.Equal(
            cmp.GetHashCode(a),
            cmp.GetHashCode(b));
    }

    [Fact]
    public void Same_Reference_Should_Be_Equal()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(x => x.Id)
            .Build();

        var a = new Person { Id = 999 };

        Assert.True(cmp.Equals(a, a));
    }

    [Fact]
    public void Nullable_Member_Should_Be_Handled()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(x => x.Name)
            .Build();

        var a = new Person { Name = null };
        var b = new Person { Name = null };

        Assert.True(cmp.Equals(a, b));
    }
}