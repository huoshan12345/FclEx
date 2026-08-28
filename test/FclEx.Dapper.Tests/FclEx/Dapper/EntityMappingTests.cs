namespace FclEx.Dapper;

public class EntityMappingTests
{
    [Fact]
    public void CreateParameter_UnrecognizedStoreTypeName_AllowsProviderInference()
    {
        var parameter = Assert.IsType<SqliteParameter>(
            new SqliteAdapter().CreateParameter("@value", "text", "varchar(200)"));

        Assert.Equal(SqliteType.Text, parameter.SqliteType);
    }

    [Fact]
    public void DataAnnotationsSource_BuildsCompletePersistentMapping()
    {
        var source = new DataAnnotationsEntityMappingSource();
        var adapter = new NpgsqlAdapter();

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
            adapter,
            null,
            typeof(AnnotatedEntity),
            source));
        Assert.Equal("\"override\".\"annotated_rows\"", DapperHelper.GetTableNameWithSchema(
            adapter,
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
        var commandOptions = new CommandOptions { EntityMappingSource = source };
        var originalTypeMap = SqlMapper.GetTypeMap(typeof(CustomMappedEntity));
        Assert.IsType<DefaultTypeMap>(originalTypeMap);
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

        var id = await connection.InsertAsync<CustomMappedEntity, long>(entity, commandOptions: commandOptions);
        var persisted = await connection.GetAsync<CustomMappedEntity>(id, commandOptions: commandOptions);

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

        Assert.Equal(1, await connection.DeleteAsync<CustomMappedEntity>(id, commandOptions: commandOptions));
        Assert.Null(await connection.GetAsync<CustomMappedEntity>(id, commandOptions: commandOptions));
        Assert.Same(originalTypeMap, SqlMapper.GetTypeMap(typeof(CustomMappedEntity)));
    }

    [Fact]
    public async Task TransactionCrud_AcceptsEntityMappingSourceThroughCommandOptions()
    {
        var commandOptions = new CommandOptions
        {
            EntityMappingSource = new SingleEntityMappingSource(CreateCustomMapping()),
        };
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
        using var transaction = connection.BeginTransaction();

        var id = await transaction.InsertAsync<CustomMappedEntity, long>(
            new CustomMappedEntity { Name = "transaction mapping" },
            commandOptions: commandOptions);
        var persisted = await transaction.GetAsync<CustomMappedEntity>(id, commandOptions: commandOptions);

        Assert.Equal("transaction mapping", persisted?.Name);
        Assert.Equal(1, await transaction.DeleteAsync<CustomMappedEntity>(id, commandOptions: commandOptions));
    }

    [Fact]
    public async Task DifferentMappingSources_ForSameEntityType_KeepSqlCachesIsolated()
    {
        var firstMapping = CreateCustomMapping("first_rows", "first_id", "first_name");
        var secondMapping = CreateCustomMapping("second_rows", "second_id", "second_name");
        var firstOptions = new CommandOptions
        {
            EntityMappingSource = new SingleEntityMappingSource(firstMapping),
        };
        var secondOptions = new CommandOptions
        {
            EntityMappingSource = new SingleEntityMappingSource(secondMapping),
        };
        var originalTypeMap = SqlMapper.GetTypeMap(typeof(CustomMappedEntity));
        Assert.IsType<DefaultTypeMap>(originalTypeMap);
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            """
            CREATE TABLE first_rows
            (
                first_id INTEGER PRIMARY KEY AUTOINCREMENT,
                first_name TEXT NOT NULL,
                generated_value INTEGER NOT NULL DEFAULT 11
            );
            CREATE TABLE second_rows
            (
                second_id INTEGER PRIMARY KEY AUTOINCREMENT,
                second_name TEXT NOT NULL,
                generated_value INTEGER NOT NULL DEFAULT 22
            );
            """);

        var firstId = await connection.InsertAsync<CustomMappedEntity, long>(
            new CustomMappedEntity { Name = "first" },
            commandOptions: firstOptions);
        var secondId = await connection.InsertAsync<CustomMappedEntity, long>(
            new CustomMappedEntity { Name = "second" },
            commandOptions: secondOptions);

        var first = await connection.GetAsync<CustomMappedEntity>(firstId, commandOptions: firstOptions);
        var second = await connection.GetAsync<CustomMappedEntity>(secondId, commandOptions: secondOptions);

        Assert.NotNull(first);
        Assert.Equal("first", first.Name);
        Assert.Equal(11, first.GeneratedValue);
        Assert.NotNull(second);
        Assert.Equal("second", second.Name);
        Assert.Equal(22, second.GeneratedValue);
        Assert.Same(originalTypeMap, SqlMapper.GetTypeMap(typeof(CustomMappedEntity)));
    }

    private static EntityMapping CreateCustomMapping(
        string tableName = "custom_rows",
        string keyColumnName = "row_id",
        string nameColumnName = "stored_name")
    {
        var type = typeof(CustomMappedEntity);
        return new EntityMapping(
            type,
            tableName,
            [
                new(type.GetRequiredProperty(nameof(CustomMappedEntity.Id)), keyColumnName, true, DatabaseValueGeneration.OnInsert),
                new(type.GetRequiredProperty(nameof(CustomMappedEntity.Name)), nameColumnName),
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
