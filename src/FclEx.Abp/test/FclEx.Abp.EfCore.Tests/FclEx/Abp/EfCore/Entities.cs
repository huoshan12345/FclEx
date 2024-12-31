using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FclEx.Abp.Domain;

namespace FclEx.Abp.EfCore;

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

[Orm.Index(true, nameof(Name))]
[Orm.Index(false, nameof(Value))]
public class EntityWithIdAndIndex : IEntity<int>
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = "";
    public int Value { get; set; }
}