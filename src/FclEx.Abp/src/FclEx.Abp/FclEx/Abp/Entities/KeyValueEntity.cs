using System.ComponentModel.DataAnnotations;
using FclEx.Abp.Orm;

namespace FclEx.Abp.Entities;

[Index(false, nameof(ObjectId))]
[Index(false, nameof(Key))]
public abstract class KeyValueEntity<TPrimaryKey> : CommonEntity<TPrimaryKey>
{
    [Required]
    public string ObjectId { get; set; } = string.Empty;

    [Required]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;
}

public class KeyValueEntity : KeyValueEntity<long>
{

}