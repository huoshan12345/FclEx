using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace FclEx.Wmi;

public static class ManagementObjectSearcherExtensions
{
    public static IEnumerable<ManagementObject> Enumerate(this ManagementObjectSearcher searcher)
    {
        return searcher.Get().Cast<ManagementObject>();
    }
}