using Microsoft.Data.Sqlite;
using FclEx.Dapper.SqlAdapters;

namespace FclEx.Dapper;

public class EntityMappingTests
{
    [Fact]
    public void CreateParameter_UnrecognizedStoreTypeName_AllowsProviderInference()
    {
        var parameter = Assert.IsType<SqliteParameter>(
            SqliteAdapter.Instance.CreateParameter("@value", "text", "varchar(200)"));

        Assert.Equal(SqliteType.Text, parameter.SqliteType);
    }

    [Fact]
    public void DataAnnotationsSource_BuildsCompletePersistentMapping()
    {
        var source = new DataAnnotationsEntityMappingSource();

        var mapping = source.GetMapping(typeof(AnnotatedEntity));

        Assert.Same(mapping, source.GetMapping(typeof(AnnotatedEntity)));
        Assert.Equal("annotated_rows", mapping.TableName);
        Assert.Equal("audit", mapping.Schema);
        Assert.Equal([nameof(AnnotatedEntity.Id), nameof(AnnotatedEntity.Name), nameof(AnnotatedEntity.Computed)],
            mapping.Properties.Select(property => property.Property.Name));
        Assert.Equal("row_id", mapping.Keys.Single().ColumnName);
        Assert.Equal("INTEGER", mapping.Keys.Single().StoreTypeName);
        Assert.Equal(DatabaseValueGeneration.OnInsert, mapping.Keys.Single().ValueGeneration);
        Assert.Equal([nameof(AnnotatedEntity.Name)],
            mapping.InsertProperties.Select(property => property.Property.Name));
        Assert.Equal(DatabaseValueGeneration.OnInsertOrUpdate,
            mapping.FindProperty(nameof(AnnotatedEntity.Computed))?.ValueGeneration);
        Assert.Equal(nameof(AnnotatedEntity.Name), mapping.FindProperty("DISPLAY_NAME")?.Property.Name);
        Assert.Equal("\"audit\".\"annotated_rows\"", DapperHelper.GetTableNameWithSchema(
            NpgsqlAdapter.Instance,
            null,
            typeof(AnnotatedEntity),
            source));
        Assert.Equal("\"override\".\"annotated_rows\"", DapperHelper.GetTableNameWithSchema(
            NpgsqlAdapter.Instance,
            "override",
            typeof(AnnotatedEntity),
            source));
        Assert.Null(mapping.FindProperty(nameof(AnnotatedEntity.Ignored)));
        Assert.Null(mapping.FindProperty(nameof(AnnotatedEntity.Navigation)));
        Assert.Null(mapping.FindProperty(nameof(AnnotatedEntity.ReadOnly)));
        Assert.Null(mapping.FindProperty("Item"));
    }

    [Fact]
    public async Task CustomMappingSource_DrivesSqliteCrudWithoutDataAnnotationsOrGlobalTypeMap()
    {
        var mapping = CreateCustomMapping();
        var source = new SingleEntityMappingSource(mapping);
        var commandInfo = new CommandInfo(EntityMappingSource: source);
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            """
            CREATE TABLE custom_rows
            (
                row_id INTEGER PRIMARY KEY AUTOINCREMENT,
                stored_name TEXT NOT NULL,
                generated_value INTEGER NOT NULL DEFAULT 7
            );
            """);

        var entity = new CustomMappedEntity
        {
            Name = "mapped explicitly",
            Ignored = "must not be inserted",
        };

        var id = (long?)await connection.InsertAsync(entity, commandInfo: commandInfo);
        var persisted = await connection.GetAsync<CustomMappedEntity>(id!.Value, commandInfo: commandInfo);

        Assert.NotNull(persisted);
        Assert.Equal(entity.Name, persisted.Name);
        Assert.Equal(7, persisted.GeneratedValue);
        Assert.Null(persisted.Ignored);
        Assert.Equal("\"stored_name\"", DapperHelper.GetQuotedColumnName(
            connection,
            typeof(CustomMappedEntity),
            nameof(CustomMappedEntity.Name),
            source));
        Assert.Equal("\"stored_name\"", DapperHelper.GetQuotedColumnName<CustomMappedEntity>(
            connection,
            mapped => mapped.Name,
            source));

        Assert.Equal(1, await connection.DeleteAsync<CustomMappedEntity>(id.Value, commandInfo: commandInfo));
        Assert.Null(await connection.GetAsync<CustomMappedEntity>(id.Value, commandInfo: commandInfo));
    }

    private static EntityMapping CreateCustomMapping()
    {
        var type = typeof(CustomMappedEntity);
        return new EntityMapping(
            type,
            "custom_rows",
            [
                new(type.GetRequiredProperty(nameof(CustomMappedEntity.Id)), "row_id", true, DatabaseValueGeneration.OnInsert),
                new(type.GetRequiredProperty(nameof(CustomMappedEntity.Name)), "stored_name"),
                new(type.GetRequiredProperty(nameof(CustomMappedEntity.GeneratedValue)), "generated_value", valueGeneration: DatabaseValueGeneration.OnInsertOrUpdate),
            ]);
    }

    [Table("annotated_rows", Schema = "audit")]
    private sealed class AnnotatedEntity
    {
        [Key]
        [Column("row_id", TypeName = "INTEGER")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("display_name")]
        public string Name { get; set; } = "";

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Computed { get; set; }

        [NotMapped]
        public string? Ignored { get; set; }

        public NavigationEntity? Navigation { get; set; }

        public string ReadOnly => Name;

        public string this[int index]
        {
            get => index.ToString();
            set { }
        }
    }

    private sealed class NavigationEntity;

    private sealed class SingleEntityMappingSource(EntityMapping mapping) : IEntityMappingSource
    {
        public EntityMapping GetMapping(Type entityType)
        {
            return entityType == mapping.EntityType
                ? mapping
                : throw new KeyNotFoundException($"No mapping is registered for '{entityType.FullName}'.");
        }
    }

    private sealed class CustomMappedEntity
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public int GeneratedValue { get; set; }
        public string? Ignored { get; set; }
    }
}
