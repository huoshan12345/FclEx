namespace FclEx.Databases;

public class DatabaseTests
{
    public static ITestOutputHelper? Output => TestContext.Current.TestOutputHelper;
    public static readonly TheoryData<DbDriver> DbDriverCases = DbDrivers.ToTheoryData();
}