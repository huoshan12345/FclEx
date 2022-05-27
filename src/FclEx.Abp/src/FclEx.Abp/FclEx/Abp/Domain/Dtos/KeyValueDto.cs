using System.ComponentModel.DataAnnotations;
using AutoMapper;
using FclEx.Abp.Domain.Entities;

namespace FclEx.Abp.Domain.Dtos
{
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
}
