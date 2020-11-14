using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xunit
{
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Global")]
    partial class AssertExt
    {
        public static void True(bool condition, Func<string>? userMessage = null)
        {
            if (!condition)
                Assert.False(condition, userMessage?.Invoke());
        }

        public static void False(bool condition, Func<string>? userMessage = null)
        {
            if (condition)
                Assert.False(condition, userMessage?.Invoke());
        }
    }
}
