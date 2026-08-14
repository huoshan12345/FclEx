using System.IO.Compression;

namespace FclEx.Extensions;

public class ZipArchiveEntryExtensionsTests
{
    [Fact]
    public async Task ExtractToDirAsync_PathTraversal_ThrowsWithoutWritingOutsideDestination()
    {
        var testDirectory = new DirectoryInfo(Path.Combine(".", nameof(ExtractToDirAsync_PathTraversal_ThrowsWithoutWritingOutsideDestination)));
        testDirectory.CreateNew();
        var outsideFile = new FileInfo(Path.Combine(testDirectory.Parent!.FullName, $"outside-{Guid.NewGuid():N}.txt"));

        try
        {
            using var stream = new MemoryStream();
            using (var writeArchive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var maliciousEntry = writeArchive.CreateEntry("../" + outsideFile.Name);
                using var writer = maliciousEntry.Open();
                var bytes = Encoding.UTF8.GetBytes("malicious");
                await writer.WriteAsync(bytes, 0, bytes.Length);
            }

            stream.Position = 0;
            using var readArchive = new ZipArchive(stream, ZipArchiveMode.Read);
            var entry = Assert.Single(readArchive.Entries);

            await Assert.ThrowsAsync<InvalidDataException>(() =>
                entry.ExtractToDirAsync(testDirectory.FullName, false, true));
            Assert.False(outsideFile.Exists);
        }
        finally
        {
            testDirectory.Refresh();
            if (testDirectory.Exists)
                testDirectory.Delete(true);
            outsideFile.Refresh();
            if (outsideFile.Exists)
                outsideFile.Delete();
        }
    }

    public static readonly string[] ZipFiles =
    [
        "files.zip",
        "dir-files.zip",
        "dir-files-nested.zip",
    ];

    public static readonly TheoryData<string> ZipFileCases = ZipFiles.ToTheoryData();

    public static readonly TheoryData<string, bool> TestCasesOfExtractToDir = ZipFiles
        .CrossJoin([false, true])
        .Select(m => (m.Item1, m.Item2))
        .ToTheoryData();

    [Theory]
    [MemberData(nameof(ZipFileCases))]
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

    [Fact]
    public void BuildTree_MissingDirectoryEntriesAndRepeatedLocalNames_CreatesIndependentBranches()
    {
        using var stream = new MemoryStream();
        using (var writeArchive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            writeArchive.CreateEntry("left/common/left.txt");
            writeArchive.CreateEntry("right/common/right.txt");
            writeArchive.CreateEntry("left/");
        }

        stream.Position = 0;
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var root = archive.BuildTree();

        var left = Assert.Single(root.Children, node => node.Value.Name == "left");
        var right = Assert.Single(root.Children, node => node.Value.Name == "right");
        Assert.False(left.Value.IsSynthetic);
        Assert.True(right.Value.IsSynthetic);

        var leftCommon = Assert.Single(left.Children);
        var rightCommon = Assert.Single(right.Children);
        Assert.Equal("left/common", leftCommon.Value.FullName);
        Assert.Equal("right/common", rightCommon.Value.FullName);
        Assert.True(leftCommon.Value.IsSynthetic);
        Assert.True(rightCommon.Value.IsSynthetic);
        Assert.Equal("left.txt", Assert.Single(leftCommon.Children).Value.Name);
        Assert.Equal("right.txt", Assert.Single(rightCommon.Children).Value.Name);
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
