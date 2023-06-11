using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO;

public static class DirectoryInfoExtensions
{
    public static DirectoryInfo EnsureExists(this DirectoryInfo di)
    {
        if (!di.Exists)
            di.Create();
        return di;
    }
}