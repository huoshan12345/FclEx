namespace FclEx.Dapper;

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