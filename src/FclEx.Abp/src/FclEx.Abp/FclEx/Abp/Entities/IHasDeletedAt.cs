using System;

namespace FclEx.Abp.Entities;

public interface IHasDeletedAt
{
    DateTimeOffset DeletedAt { get; set; }
}