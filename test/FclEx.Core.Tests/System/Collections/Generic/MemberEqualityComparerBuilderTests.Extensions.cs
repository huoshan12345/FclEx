namespace System.Collections.Generic;

partial class MemberEqualityComparerBuilderTests
{
    public record TestModel(int Id, bool IsDirectory, long Length, string Name, string? FullPath);

    [Fact]
    public void AddAllDataMembers_Test()
    {
        var comparer = MemberEqualityComparerBuilder<TestModel>
            .Create()
            .AddAllDataMembers(false, nameof(TestModel.Id))
            .Build();

        var random = new Random(0);

        for (var i = 0; i < 1000; i++)
        {
            var path = random.NextBoolean() ? null : random.NextString(10);
            var x = new TestModel(random.Next(), random.NextBoolean(), random.NextInt64(), random.NextString(10), path);
            var y = ObjectHelper.CloneByJson(x);
            Assert.Equal(x, y);

            Assert.Equal(x, y, comparer);
            {
                var set = new HashSet<TestModel>([x], comparer);
                Assert.Contains(y, set);
            }
            {
                var z = y with { Id = y.Id + 1 };
                Assert.Equal(y, z, comparer);

                var set = new HashSet<TestModel>([y], comparer);
                Assert.Contains(z, set);
            }
            {
                var z = y with { IsDirectory = !y.IsDirectory };
                Assert.NotEqual(y, z, comparer);

                var set = new HashSet<TestModel>([y], comparer);
                Assert.DoesNotContain(z, set);
            }
            {
                var z = y with { Length = y.Length + 1 };
                Assert.NotEqual(y, z, comparer);

                var set = new HashSet<TestModel>([y], comparer);
                Assert.DoesNotContain(z, set);
            }
            {
                var z = y with { Name = y.Name + 1 };
                Assert.NotEqual(y, z, comparer);

                var set = new HashSet<TestModel>([y], comparer);
                Assert.DoesNotContain(z, set);
            }
            {
                var z = y with { FullPath = y.FullPath + 1 };
                Assert.NotEqual(y, z, comparer);

                var set = new HashSet<TestModel>([y], comparer);
                Assert.DoesNotContain(z, set);
            }
        }
    }

    [Fact]
    public void Add_ByName_IntMember_ShouldBeEqual()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(nameof(Person.Id))
            .Build();

        var a = new Person { Id = 1 };
        var b = new Person { Id = 1 };

        Assert.True(cmp.Equals(a, b));
    }

    [Fact]
    public void Add_ByName_IntMember_ShouldNotBeEqual()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(nameof(Person.Id))
            .Build();

        var a = new Person { Id = 1 };
        var b = new Person { Id = 2 };

        Assert.False(cmp.Equals(a, b));
    }

    [Fact]
    public void Add_ByName_StringMember_ShouldBeEqual()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(nameof(Person.Name))
            .Build();

        var a = new Person { Name = "Alice" };
        var b = new Person { Name = "Alice" };

        Assert.True(cmp.Equals(a, b));
    }

    [Fact]
    public void Add_ByName_NullMember_ShouldBeEqual()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(nameof(Person.Name))
            .Build();

        var a = new Person { Name = null };
        var b = new Person { Name = null };

        Assert.True(cmp.Equals(a, b));
    }

    [Fact]
    public void Add_ByMultipleNames_ShouldCompareAll()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(nameof(Person.Id), nameof(Person.Age))
            .Build();

        var a = new Person { Id = 1, Age = 10 };
        var b = new Person { Id = 1, Age = 20 };

        Assert.False(cmp.Equals(a, b));
    }

    [Fact]
    public void Add_ByName_ShouldWorkInDictionary()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(nameof(Person.Id))
            .Build();

        var dict = new Dictionary<Person, string>(cmp)
        {
            [new Person { Id = 1 }] = "ok"
        };

        Assert.True(dict.ContainsKey(new Person { Id = 1 }));
    }

    [Fact]
    public void Add_ByName_ShouldWorkInHashSet()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(nameof(Person.Id))
            .Build();

        var set = new HashSet<Person>(cmp)
        {
            new Person { Id = 1 }
        };

        Assert.Contains(new Person { Id = 1 }, set);
    }

    [Fact]
    public void Add_ByName_StructMembers_ShouldBeEqual()
    {
        var cmp = MemberEqualityComparerBuilder<OrderKey>
            .Create()
            .Add(nameof(OrderKey.StoreId), nameof(OrderKey.OrderId))
            .Build();

        var a = new OrderKey { StoreId = 1, OrderId = 99 };
        var b = new OrderKey { StoreId = 1, OrderId = 99 };

        Assert.True(cmp.Equals(a, b));
        Assert.Equal(
            cmp.GetHashCode(a),
            cmp.GetHashCode(b));
    }

    [Fact]
    public void Add_ByName_Null_ShouldEqualNull()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(nameof(Person.Id))
            .Build();

        Assert.True(cmp.Equals(null, null));
    }

    [Fact]
    public void Add_ByName_Instance_ShouldNotEqualNull()
    {
        var cmp = MemberEqualityComparerBuilder<Person>
            .Create()
            .Add(nameof(Person.Id))
            .Build();

        var a = new Person { Id = 1 };

        Assert.False(cmp.Equals(a, null));
    }
}
