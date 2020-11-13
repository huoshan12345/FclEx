using System.Reflection;
using FclEx;
using FclEx.Utils;

namespace Xunit
{
    public class DebugOnlyTheoryAttribute : TheoryAttribute
    {
        public bool DebugModeRequired { get; set; } = true;
        public bool DebuggerRequired { get; set; } = false;

        public override string? Skip
        {
            get
            {
                if (DebugModeRequired)
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    if (!assembly.IsDebug())
                        return $"The entry assembly {assembly.GetName().Name} is not in debug mode";
                }
                if (DebuggerRequired && !RuntimeInfo.IsDebuggerAttached)
                    return "The debugger is not attached";
                return null;
            }
        }
    }
}
