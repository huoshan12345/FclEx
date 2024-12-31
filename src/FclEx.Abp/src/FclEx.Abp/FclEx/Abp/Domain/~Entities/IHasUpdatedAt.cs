using System;

namespace FclEx.Abp.Domain;

public interface IHasUpdatedAt
{
    DateTimeOffset UpdatedAt { get; set; }
}