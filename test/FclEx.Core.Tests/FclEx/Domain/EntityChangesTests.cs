namespace FclEx.Domain;

public class EntityChangesTests
{
    [Fact]
    public void Constructor_CreatesReadOnlyDefensiveSnapshots()
    {
        var inserted = new List<string> { "inserted" };
        var updated = new List<EntityUpdate<string>> { new("new", "existing") };
        var deleted = new List<string> { "deleted" };

        var changes = new EntityChanges<string>(inserted, updated, deleted);
        inserted.Add("later");
        updated.Clear();
        deleted[0] = "changed";

        Assert.Equal(["inserted"], changes.Inserted);
        Assert.Equal([new EntityUpdate<string>("new", "existing")], changes.Updated);
        Assert.Equal(["deleted"], changes.Deleted);
        Assert.Throws<NotSupportedException>(() => ((ICollection<string>)changes.Inserted).Add("new"));
    }
}
