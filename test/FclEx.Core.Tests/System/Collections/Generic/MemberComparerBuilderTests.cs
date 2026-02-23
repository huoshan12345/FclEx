namespace System.Collections.Generic;

public class MemberComparerBuilderTests
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
    public void OrderBy_Int_ShouldSortAscending()
    {
        var cmp = MemberComparerBuilder<Person>
            .Create()
            .OrderBy(x => x.Age)
            .Build();

        var list = new List<Person>
        {
            new() { Age = 30 },
            new() { Age = 10 },
            new() { Age = 20 }
        };

        list.Sort(cmp);

        Assert.Equal(10, list[0].Age);
        Assert.Equal(20, list[1].Age);
        Assert.Equal(30, list[2].Age);
    }

    [Fact]
    public void OrderBy_Int_Desc_ShouldSortDescending()
    {
        var cmp = MemberComparerBuilder<Person>
            .Create()
            .OrderBy(x => x.Age, true)
            .Build();

        var list = new List<Person>
        {
            new() { Age = 30 },
            new() { Age = 10 },
            new() { Age = 20 }
        };

        list.Sort(cmp);

        Assert.Equal(30, list[0].Age);
        Assert.Equal(20, list[1].Age);
        Assert.Equal(10, list[2].Age);
    }

    [Fact]
    public void OrderBy_MultipleMembers_ShouldRespectOrder()
    {
        var cmp = MemberComparerBuilder<Person>
            .Create()
            .OrderBy(x => x.Id)
            .OrderBy(x => x.Age)
            .Build();

        var list = new List<Person>
        {
            new() { Id = 1, Age = 30 },
            new() { Id = 1, Age = 10 },
            new() { Id = 2, Age = 5 }
        };

        list.Sort(cmp);

        Assert.Equal(1, list[0].Id);
        Assert.Equal(10, list[0].Age);
        Assert.Equal(1, list[1].Id);
        Assert.Equal(30, list[1].Age);
        Assert.Equal(2, list[2].Id);
    }

    [Fact]
    public void OrderBy_CustomComparer_ShouldBeUsed()
    {
        var cmp = MemberComparerBuilder<Person>
            .Create()
            .OrderBy(x => x.Name, false, StringComparer.OrdinalIgnoreCase)
            .Build();

        var list = new List<Person>
        {
            new() { Name = "bob" },
            new() { Name = "Alice" }
        };

        list.Sort(cmp);

        Assert.Equal("Alice", list[0].Name);
        Assert.Equal("bob", list[1].Name);
    }

    [Fact]
    public void OrderBy_ShouldHandleNullMembers()
    {
        var cmp = MemberComparerBuilder<Person>
            .Create()
            .OrderBy(x => x.Name)
            .Build();

        var list = new List<Person>
        {
            new() { Name = "Bob" },
            new() { Name = null }
        };

        list.Sort(cmp);

        Assert.Null(list[0].Name);
        Assert.Equal("Bob", list[1].Name);
    }

    [Fact]
    public void CreateComparison_ShouldSortCorrectly()
    {
        var comparison = MemberComparerBuilder<Person>
            .Create()
            .OrderBy(x => x.Age)
            .CreateComparison();

        var list = new List<Person>
        {
            new() { Age = 2 },
            new() { Age = 1 }
        };

        list.Sort(comparison);

        Assert.Equal(1, list[0].Age);
        Assert.Equal(2, list[1].Age);
    }

    [Fact]
    public void StructMembers_ShouldCompareCorrectly()
    {
        var cmp = MemberComparerBuilder<OrderKey>
            .Create()
            .OrderBy(x => x.StoreId)
            .OrderBy(x => x.OrderId)
            .Build();

        var list = new List<OrderKey>
        {
            new() { StoreId = 1, OrderId = 20 },
            new() { StoreId = 1, OrderId = 10 }
        };

        list.Sort(cmp);

        Assert.Equal(10, list[0].OrderId);
        Assert.Equal(20, list[1].OrderId);
    }

    [Fact]
    public void SameReference_ShouldCompareAsEqual()
    {
        var cmp = MemberComparerBuilder<Person>
            .Create()
            .OrderBy(x => x.Age)
            .Build();

        var a = new Person { Age = 10 };

        Assert.Equal(0, cmp.Compare(a, a));
    }

    [Fact]
    public void Null_ShouldBeLessThan_Instance()
    {
        var cmp = MemberComparerBuilder<Person>
            .Create()
            .OrderBy(x => x.Age)
            .Build();

        var a = new Person { Age = 10 };

        Assert.True(cmp.Compare(null!, a) < 0);
    }

    [Fact]
    public void Instance_ShouldBeGreaterThan_Null()
    {
        var cmp = MemberComparerBuilder<Person>
            .Create()
            .OrderBy(x => x.Age)
            .Build();

        var a = new Person { Age = 10 };

        Assert.True(cmp.Compare(a, null!) > 0);
    }
}