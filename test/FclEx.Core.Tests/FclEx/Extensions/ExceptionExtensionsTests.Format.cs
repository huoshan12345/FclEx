namespace FclEx.Extensions;

partial class ExceptionExtensionsTests
{
    [Fact]
    public async Task BuildTree_Complex_Test()
    {
        try
        {
            await ExceptionCreator.Run();
        }
        catch (Exception ex)
        {
            var (infos, _) = ex.BuildTree();

            foreach (var info in infos)
            {
                foreach (var line in info.StackTraceLines)
                {
                    Assert.True(ExceptionExtensions.ExcludeStackTracePrefixes.All(m => line.StartsWith(m) == false));
                }
            }
        }
    }

    [Fact]
    public void GetInfo_Test()
    {
        var text = File.ReadAllText(Path.Combine("TestData", "StackTrace.txt"));
        var ex = new Exception().SetStackTrace(text);

        var index = 0;
        var info = ex.GetInfo(ref index, -1, null);

        foreach (var line in info.StackTraceLines)
        {
            Assert.True(ExceptionExtensions.ExcludeStackTracePrefixes.All(m => line.StartsWith(m) == false));
        }
    }
}