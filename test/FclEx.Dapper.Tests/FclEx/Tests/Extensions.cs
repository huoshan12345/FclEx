namespace FclEx.Tests;

public static class Extensions
{
    extension(Assert)
    {
        public static void SkipUnlessIncluded(DbDriver driver, DbDriver[] dbDrivers)
        {
            Assert.SkipUnless(
                dbDrivers.Contains(driver),
                $"Skipped because driver '{driver}' is not included in this test run (enabled drivers: {string.Join(", ", dbDrivers)}).");
        }

        public static void SkipUnlessIncluded(DbDriver driver)
        {
            Assert.SkipUnlessIncluded(driver, DbDrivers);
        }
    }
}
