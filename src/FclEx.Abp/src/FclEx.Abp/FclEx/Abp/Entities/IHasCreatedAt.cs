using System;

namespace FclEx.Abp.Entities;

public interface IHasCreatedAt
{
    DateTimeOffset CreatedAt { get; set; }
}