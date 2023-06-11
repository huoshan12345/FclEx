using System;
using FclEx.Abp.Domain.Entities.Interfaces;
using FclEx.Abp.Orm;

namespace FclEx.Abp.Domain.Entities;

public abstract class CommonEntity<TPrimaryKey> : ICommonEntity<TPrimaryKey>
{
    public DateTime CreationTime { get; set; }

    public DateTime LastModificationTime { get; set; }

    public bool IsDeleted { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public TPrimaryKey Id { get; set; } = default!;
}