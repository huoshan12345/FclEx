namespace FclEx.Tests;

public static class Extensions
{
    extension(Assert)
    {
        public static void SkipUnlessHasApiServer()
        {
            Assert.SkipUnless(HasApiServer, "API server is not available.");
        }
    }
}
