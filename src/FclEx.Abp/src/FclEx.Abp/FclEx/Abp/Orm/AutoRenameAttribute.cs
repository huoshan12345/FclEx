using System;

namespace FclEx.Abp.Orm;

[AttributeUsage(AttributeTargets.Class)]
public class AutoRenameAttribute : Attribute
{
    public bool RemoveEntitySuffix { get; set; } = true;
}