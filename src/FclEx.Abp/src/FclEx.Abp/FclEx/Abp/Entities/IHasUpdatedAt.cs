using System;

namespace FclEx.Abp.Entities;

public interface IHasUpdatedAt
{
    DateTimeOffset UpdatedAt { get; set; }
}