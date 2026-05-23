namespace FclEx.Databases;

public class EntityWithAutoKey
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Value { get; set; }
}

public class EntityWithGuidKey
{
    [Key]
    public Guid Id { get; set; }
    public int Value { get; set; }
    public int? Order { get; set; } // As order is a keyword, we use it to test if GetQuotedColumnName works well.
}

public class EntityWithoutKey
{
    public string? Name { get; set; }
    public int Value { get; set; }
}

public class EntityWithPostgresqlJsonb
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("json_string", TypeName = "jsonb")]
    public string? Json { get; set; }
}

public class EntityWithSqlServerXml
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("xml_string", TypeName = "xml")]
    public string? Xml { get; set; }
}

public class EntityWithSqliteBlob
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("blob_bytes", TypeName = "blob")]
    public byte[]? Blob { get; set; }
}

public class EntityWithMySqlBlob
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("blob_bytes", TypeName = "blob")]
    public byte[]? Blob { get; set; }
}

public class HasPostfixEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}

[Table("has_table_name")]
public class HasTableAttributeEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}

public class EntityWithIdAndIndex : IHasId<int>
{
    public int Id { get; set; }  // Automatically becomes auto-incrementing PK
    [Required]
    public string Name { get; set; } = "";
    public int Value { get; set; }
}

[DebuggerDisplay("{Id}, {Name}")]
public class EntityHasStates : SoftDeletableEntity<long>
{
    [Required]
    public string Name { get; set; } = "";

    public override string ToString()
    {
        return $"EntityHasStates(Id = {Id}, Name = {Name})";
    }
}

public class EntityWithNavigation : IHasId<long>
{
    public long Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    public long? NavigationId { get; set; }
    public EntityHasStates? Navigation { get; set; }
}