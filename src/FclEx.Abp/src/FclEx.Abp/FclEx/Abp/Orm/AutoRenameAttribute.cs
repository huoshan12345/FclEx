using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Abp.Orm
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AutoRenameAttribute : Attribute
    {
        public bool RemoveEntityPostfix { get; set; } = true;
    }
}
