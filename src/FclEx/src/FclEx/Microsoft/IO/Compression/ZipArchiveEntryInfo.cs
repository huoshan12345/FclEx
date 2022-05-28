using System;
using System.IO.Compression;
using FclEx;
using FclEx.Extensions;

namespace Microsoft.IO.Compression;

public readonly struct ZipArchiveEntryInfo
{
    public readonly ZipArchiveEntry? Entry;
    public readonly string[] Segments;
    public readonly bool IsDirectory;
    public readonly bool IsFile;
    public readonly string Name;
    public readonly string Parent;

    public ZipArchiveEntryInfo(ZipArchiveEntry entry)
    {
        Entry = entry;
        Segments = entry.FullName.Split("/", StringSplitOptions.RemoveEmptyEntries);
        IsDirectory = entry.IsDirectory();
        IsFile = !IsDirectory;
        Name = Segments[^1];
        Parent = Segments.TryGet(Segments.Length - 2, ".");
    }
}