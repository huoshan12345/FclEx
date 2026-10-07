using FclEx.Databases;

namespace Xunit;

public static class DatabaseAssertExtensions
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
            Assert.SkipUnlessIncluded(driver, SelectedDrivers);
        }
    }
}
