using System;

namespace FclEx.Abp.Domain;

public interface IHasDeletedAt
{
    DateTimeOffset DeletedAt { get; set; }
}