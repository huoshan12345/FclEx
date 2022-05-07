using System.Diagnostics;
using System.Reflection;

namespace FclEx
{
    public static class AssemblyExtensions
    {
        private static (string BuildType, bool IsJitOptimized) GetInfo(this Assembly assembly)
        {
            var (buildType, isJitOptimized) = ("Debug", false);
            var attr = assembly.GetCustomAttribute<DebuggableAttribute>();

            // If the 'DebuggableAttribute' is not found then it is definitely an OPTIMIZED build
            if (attr != null)
            {
                // Just because the 'DebuggableAttribute' is found doesn't necessarily mean
                // it's a DEBUG build; we have to check the JIT Optimization flag
                // i.e. it could have the "generate PDB" checked but have JIT Optimization enabled
                isJitOptimized = !attr.IsJITOptimizerDisabled;
                buildType = attr.IsJITOptimizerDisabled ? "Debug" : "Release";

                // check for Debug Output "full" or "pdb-only"
                //DebugOutput = (debuggableAttribute.DebuggingFlags &
                //               DebuggableAttribute.DebuggingModes.Default) !=
                //              DebuggableAttribute.DebuggingModes.None
                //    ? "Full" : "pdb-only";
            }
            else
            {
                isJitOptimized = true;
                buildType = "Release";
            }

            return (buildType, isJitOptimized);
        }

        public static bool IsDebug(this Assembly assembly)
        {
            return assembly.GetInfo().BuildType == "Debug";
        }

        public static bool IsRelease(this Assembly assembly)
        {
            return assembly.GetInfo().BuildType == "Release";
        }
    }
}
