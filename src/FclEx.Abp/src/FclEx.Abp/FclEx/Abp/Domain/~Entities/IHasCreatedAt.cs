using System;

namespace FclEx.Abp.Domain;

public interface IHasCreatedAt
{
    DateTimeOffset CreatedAt { get; set; }
}