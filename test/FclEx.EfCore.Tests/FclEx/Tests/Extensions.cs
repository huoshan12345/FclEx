namespace FclEx.Tests;

public static class Extensions
{
    extension(Assert)
    {
        public static void SkipUnlessIncluded(DbDriver driver)
        {
            Assert.SkipUnless(
                DbDrivers.Contains(driver),
                $"Skipped because driver '{driver}' is not included in this test run (enabled drivers: {string.Join(", ", DbDrivers)}).");
        }
    }
}
