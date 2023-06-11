using System;

namespace FclEx.Abp.Domain.Entities.Interfaces;

public interface IHasDeletionTime : ISoftDelete
{
    DateTime DeletionTime { get; set; }
}