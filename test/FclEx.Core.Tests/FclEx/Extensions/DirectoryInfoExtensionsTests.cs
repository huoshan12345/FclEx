namespace FclEx.Extensions;

public class DirectoryInfoExtensionsTests
{
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