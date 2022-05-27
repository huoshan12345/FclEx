using System;

namespace FclEx.Abp.Domain.Entities.Interfaces
{
    public interface IHasCreationTime
    {
        DateTime CreationTime { get; set; }
    }
}