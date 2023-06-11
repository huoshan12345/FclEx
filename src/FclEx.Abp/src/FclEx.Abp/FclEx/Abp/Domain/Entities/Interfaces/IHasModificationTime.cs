using System;

namespace FclEx.Abp.Domain.Entities.Interfaces;

public interface IHasModificationTime
{
    DateTime LastModificationTime { get; set; }
}