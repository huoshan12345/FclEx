namespace FclEx.Domain;

[Collection("EntityDataEquality")]
public class EntityDataEqualityComparerTests
{
    [Fact]
    public void DefaultComparer_IgnoresAuditTimes_ButDetectsDataChanges()
    {
        var comparer = EntityDataEqualityComparer.Create<Model>();
        var first = new Model { Id = 1, Name = "first" };
        var second = new Model
        {
            Id = 1,
            Name = "first",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            DeletedAt = DateTimeOffset.UtcNow,
        };

        Assert.True(comparer.Equals(first, second));
        Assert.Equal(comparer.GetHashCode(first), comparer.GetHashCode(second));
        Assert.Contains(second, new HashSet<Model>(comparer) { first });
        second.Name = "changed";
        Assert.False(comparer.Equals(first, second));
        second.Name = first.Name;
        second.Id = 2;
        Assert.False(comparer.Equals(first, second));
    }

    [Fact]
    public void Create_NullOrEquivalentDefaults_ReturnsCachedInstance()
    {
        var names = EntityDataEqualityComparer.DefaultExcludedNames.Reverse().ToArray();
        Assert.Same(EntityDataEqualityComparer<Model>.Instance, EntityDataEqualityComparer<Model>.Create());
        Assert.Same(EntityDataEqualityComparer<Model>.Instance, EntityDataEqualityComparer.Create<Model>(null));
        Assert.Same(EntityDataEqualityComparer<Model>.Instance, EntityDataEqualityComparer<Model>.Create(names));
        Assert.Same(EntityDataEqualityComparer<Model>.Instance, EntityDataEqualityComparer.Create<Model>(names.Concat(names).ToArray()));
    }

    [Theory]
    [InlineData(nameof(Model.CreatedAt))]
    [InlineData(nameof(Model.UpdatedAt))]
    [InlineData(nameof(Model.DeletedAt))]
    public void CustomExclusions_ReplaceDefaults(string memberName)
    {
        var first = new Model { Name = "first" };
        var second = new Model { Name = "second" };
        typeof(Model).GetProperty(memberName)!.SetValue(second, DateTimeOffset.UtcNow);

        Assert.False(EntityDataEqualityComparer.Create<Model>([nameof(Model.Name)]).Equals(first, second));
        Assert.True(EntityDataEqualityComparer.Create<Model>([nameof(Model.Name), memberName]).Equals(first, second));
    }

    [Theory]
    [InlineData(nameof(Model.Name), true)]
    [InlineData("name", false)]
    [InlineData("Unknown", false)]
    public void ExclusionNames_AreCaseSensitive_AndUnknownNamesAreIgnored(string name, bool expected)
    {
        var comparer = EntityDataEqualityComparer.Create<Model>([name]);
        Assert.Equal(expected, comparer.Equals(new Model { Name = "a" }, new Model { Name = "b" }));
    }

    [Fact]
    public void Create_EmptyExclusions_IncludesAuditTimes()
    {
        var comparer = EntityDataEqualityComparer.Create<Model>([]);
        Assert.False(comparer.Equals(new Model(), new Model { UpdatedAt = DateTimeOffset.UtcNow }));
    }

    [Fact]
    public void Create_DoesNotTrackChangesToSuppliedNames()
    {
        var names = new List<string> { nameof(Model.Name) };
        var comparer = EntityDataEqualityComparer.Create<Model>(names);
        names.Clear();

        var first = new Model { Name = "a" };
        var second = new Model { Name = "b" };
        Assert.True(comparer.Equals(first, second));
        Assert.Equal(comparer.GetHashCode(first), comparer.GetHashCode(second));
        Assert.False(EntityDataEqualityComparer.Create<Model>(names).Equals(first, second));
    }

    [Fact]
    public void Equals_HandlesNullAndIdenticalReferences()
    {
        var comparer = EntityDataEqualityComparer<Model>.Instance;
        var entity = new Model();
        Assert.True(comparer.Equals(null!, null!));
        Assert.False(comparer.Equals(null!, entity));
        Assert.False(comparer.Equals(entity, null!));
        Assert.True(comparer.Equals(entity, entity));
    }

    [Fact]
    public void Equals_UsesDeclaredTypeMembers_AndRejectsDifferentRuntimeTypes()
    {
        var comparer = EntityDataEqualityComparer<Parent>.Instance;
        Assert.True(comparer.Equals(new Child { Extra = 1 }, new Child { Extra = 2 }));
        Assert.False(comparer.Equals(new Parent(), new Child()));
        Assert.False(EntityDataEqualityComparer<Child>.Instance.Equals(new Child { Value = 1 }, new Child { Value = 2 }));
    }

    [Fact]
    public void Equals_UsesDefaultCollectionEquality()
    {
        var comparer = EntityDataEqualityComparer<CollectionModel>.Instance;
        var shared = new List<int> { 1, 2 };
        Assert.True(comparer.Equals(new CollectionModel { Items = shared }, new CollectionModel { Items = shared }));
        Assert.False(comparer.Equals(new CollectionModel { Items = [1, 2] }, new CollectionModel { Items = [1, 2] }));
    }

    [Fact]
    public void Equals_IncludesPublicFields_ButExcludesPrivateMembersAndIndexers()
    {
        var comparer = EntityDataEqualityComparer<MemberModel>.Instance;
        var first = new MemberModel(1) { Value = 3 };
        var second = new MemberModel(2) { Value = 3 };
        Assert.True(comparer.Equals(first, second));
        Assert.Equal(comparer.GetHashCode(first), comparer.GetHashCode(second));
        second.Value = 4;
        Assert.False(comparer.Equals(first, second));
    }

    [Fact]
    public void DefaultExclusions_AreReadOnly_AndCustomPoliciesDoNotChangeDefaults()
    {
        var cached = EntityDataEqualityComparer<CachedModel>.Instance;
        var defaults = EntityDataEqualityComparer.DefaultExcludedNames;
        Assert.False((object)defaults is ISet<string>);
        Assert.False((object)defaults is ICollection<string>);
        Assert.True(defaults.SetEquals(["CreatedAt", "UpdatedAt", "DeletedAt"]));

        var custom = EntityDataEqualityComparer.Create<CachedModel>([nameof(CachedModel.ConfigurationProbe)]);
        var first = new CachedModel { ConfigurationProbe = 1 };
        var second = new CachedModel { ConfigurationProbe = 2 };
        Assert.True(custom.Equals(first, second));
        Assert.False(cached.Equals(first, second));
        Assert.Same(cached, EntityDataEqualityComparer.Create<CachedModel>(defaults.ToArray()));
    }

    public class Model
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset DeletedAt { get; set; }
    }

    public class Parent
    {
        public int Value { get; set; }
    }

    public class Child : Parent
    {
        public int Extra { get; set; }
    }

    public class CollectionModel
    {
        public List<int>? Items { get; set; }
    }

    public class MemberModel(int secret)
    {
        private int Secret { get; } = secret;
        public int Value;
        public int this[int index] => throw new InvalidOperationException("Indexers must not be read.");
    }

    public class CachedModel
    {
        public int ConfigurationProbe { get; set; }
    }

}

[CollectionDefinition("EntityDataEquality", DisableParallelization = true)]
public class EntityDataEqualityCollection;
