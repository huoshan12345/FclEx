using AutoMapper;
using FclEx.Abp.Entities;

namespace FclEx.Abp.Dtos;

public abstract class KeyValueDto<TPrimaryKey> : EntityDto<TPrimaryKey>
{
    public string ObjectId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    protected KeyValueDto() { }

    protected KeyValueDto(string objectId, string key, string value)
    {
        Key = key;
        Value = value;
        ObjectId = objectId;
    }
}

[AutoMap(typeof(KeyValueEntity), ReverseMap = true)]
public class KeyValueDto : KeyValueDto<long>
{
    public KeyValueDto() { }

    public KeyValueDto(string objectId, string key, string value)
        : base(objectId, key, value) { }
}