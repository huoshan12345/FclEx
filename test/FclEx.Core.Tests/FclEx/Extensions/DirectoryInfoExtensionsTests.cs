namespace FclEx.Extensions;

public class DirectoryInfoExtensionsTests
{
    [Fact]
    public void SubAndFile_OnlyAcceptDirectChildNames()
    {
        var directory = new DirectoryInfo(Path.GetTempPath());

        Assert.Equal(Path.Combine(directory.FullName, "child"), directory.Sub("child").FullName);
        Assert.Equal(Path.Combine(directory.FullName, "file.txt"), directory.File("file.txt").FullName);
        Assert.Throws<ArgumentException>(() => directory.Sub(".."));
        Assert.Throws<ArgumentException>(() => directory.Sub(Path.Combine("nested", "child")));
        Assert.Throws<ArgumentException>(() => directory.File(Path.GetFullPath("file.txt")));
    }

    [Fact]
    public void Rename_OnlyAcceptsDirectChildNames()
    {
        var directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));

        Assert.Throws<ArgumentException>(() => directory.Rename(".."));
        Assert.Throws<ArgumentException>(() => directory.Rename(Path.Combine("nested", "child")));
        Assert.Throws<ArgumentException>(() => directory.Rename(Path.GetTempPath()));
    }

    [Fact]
    public void IsDescendantOf_UsesPathBoundariesAndExcludesSelf()
    {
        var parent = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "parent"));

        Assert.True(new DirectoryInfo(Path.Combine(parent.FullName, "child", "grandchild")).IsDescendantOf(parent));
        Assert.False(new DirectoryInfo(parent.FullName).IsDescendantOf(parent));
        Assert.False(new DirectoryInfo(parent.FullName + "-sibling").IsDescendantOf(parent));
    }

    [Fact]
    public void Recreate_RemovesExistingContents()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        File.WriteAllText(Path.Combine(directory.FullName, "old.txt"), "old");

        try
        {
            var result = directory.Recreate();

            Assert.True(result.Exists);
            Assert.True(result.IsEmpty());
        }
        finally
        {
            directory.TryDelete(true);
        }
    }

    [Fact]
    public void MoveToRecycleBin_Test()
    {
        var path = Path.Combine(nameof(MoveToRecycleBin_Test), "test.txt");
        var fi = new FileInfo(path);
        fi.Directory!.TryCreate();
        File.WriteAllText(path, "xxxxxxxxxx");
        Assert.True(fi.Exists);
        fi.Delete();
    }
}
