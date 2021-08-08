using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Helpers
{
    public static class AssemblyHelper
    {
        public static readonly Assembly AssemblyOfAction = typeof(Action).Assembly;
        public static readonly Assembly AssemblyOfFunc = typeof(Func<>).Assembly;
    }
}
