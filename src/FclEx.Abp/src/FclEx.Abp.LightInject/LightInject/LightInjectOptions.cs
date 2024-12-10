using System;
using System.Text;

namespace LightInject;

public class LightInjectOptions
{
    public bool UseAop { get; set; } = true;
    public bool EnableDebuggerLogging { get; set; } = false;
    public bool ThrowOnWarningLog { get; set; } = false;

    internal static LightInjectOptions Default { get; } = new();
}