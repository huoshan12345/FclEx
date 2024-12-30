using System;

namespace FclEx.Abp.Orm;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AutoRenameAttribute : Attribute
{
    public bool RemoveEntityPostfix { get; set; } = true;
}