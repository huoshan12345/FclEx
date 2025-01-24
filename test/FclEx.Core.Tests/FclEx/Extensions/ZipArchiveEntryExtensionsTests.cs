using System.IO.Compression;

namespace FclEx.Extensions;

public class ZipArchiveEntryExtensionsTests
{
    public static readonly string[] ZipFiles =
    [
        "files.zip",
        "dir-files.zip",
        "dir-files-nested.zip",
    ];

    public static readonly IEnumerable<object[]> TestCasesOfExtractToDir = ZipFiles
        .CrossJoin([false, true])
        .Select(m => new object[] { m.Item1, m.Item2 });

    [Theory]
    [InlineData("files.zip")]
    [InlineData("dir-files.zip")]
    [InlineData("dir-files-nested.zip")]
    public void BuildTree_Test(string zipFile)
    {
        var zip = Path.Combine("TestData", zipFile);
        using var archive = ZipFile.Open(zip, ZipArchiveMode.Read);
        var dir = new DirectoryInfo(Path.Combine(".", nameof(BuildTree_Test), zipFile));
        dir.CreateNew();
        archive.ExtractToDirectory(dir.FullName);
        var root = archive.BuildTree();

        var queue = new Queue<(DirectoryInfo, TreeNode<ZipArchiveEntryInfo>)>();
        queue.Enqueue((dir, root));
        while (queue.Any())
        {
            var (di, node) = queue.Dequeue();
            CheckNode(di, node);

            foreach (var pair in di.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).Zip(node.Children))
            {
                queue.Enqueue(pair);
            }
        }
    }

    private static void CheckNode(DirectoryInfo di, TreeNode<ZipArchiveEntryInfo> node)
    {
        var files = di.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
            .Select(m => (m is DirectoryInfo, m.Name))
            .OrderBy(m => !m.Item1)
            .ThenBy(m => m.Name);
        var values = node.Children.Select(m => (m.Value.IsDirectory, m.Value.Name));
        Assert.True(files.SequenceEqual(values));
    }

    [Theory]
    [MemberData(nameof(TestCasesOfExtractToDir))]
    public async Task ExtractToDir_Test(string zipFile, bool ignoreEntryDir)
    {
        var dirName = zipFile + "_" + (ignoreEntryDir ? "ignore" : "");
        var dir = new DirectoryInfo(Path.Combine(".", nameof(ExtractToDir_Test), dirName));
        dir.CreateNew();

        var zip = Path.Combine("TestData", zipFile);
        using var archive = ZipFile.Open(zip, ZipArchiveMode.Read);

        foreach (var m in archive.Entries.Where(m => m.Name != ""))
        {
            await m.ExtractToDirAsync(dir.FullName, ignoreEntryDir, false);
        }

        var (fileCount, dirCount) = dir.EnumerateFileSystemInfos("*", ignoreEntryDir ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories)
            .Partition(m => m is FileInfo, m => m.Count());

        var (fileCountExpected, dirCountExpected) = archive.Entries.Partition(m => m.Name != "", m => m.Count());
        Assert.Equal(fileCountExpected, fileCount);
        Assert.Equal(ignoreEntryDir ? 0 : dirCountExpected, dirCount);
    }
}