namespace FclEx.Databases;

public class DatabaseTests
{
    public static ITestOutputHelper? Output => TestContext.Current.TestOutputHelper;
    public static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
    public static readonly TheoryData<DbDriver> DbDriverCases = SelectedDrivers.ToTheoryData();
}