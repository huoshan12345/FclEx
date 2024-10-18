namespace System.IO.Compression;

public class ZipArchiveEntryInfo
{
    public readonly ZipArchiveEntry Entry;
    public readonly string[] Segments;
    public readonly bool IsDirectory;
    public readonly bool IsFile;
    public readonly string Name;
    public readonly string Parent;

    public ZipArchiveEntryInfo(ZipArchiveEntry entry)
    {
        Entry = Check.NotNull(entry);
        Segments = entry.FullName.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
        IsDirectory = entry.IsDirectory();
        IsFile = !IsDirectory;
        Name = Segments[^1];
        Parent = Segments.Get(Segments.Length - 2, ".");
    }
}