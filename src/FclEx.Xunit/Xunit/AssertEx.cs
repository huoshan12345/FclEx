namespace Xunit;

public partial class AssertEx
{
    extension(Assert)
    {
#if FCLEX_XUNIT_V3
        public static void SkipIfInGithubAction()
        {
            if (TestHelper.IsGithubAction)
                Assert.Skip("Skipping test in GitHub Action.");
        }
#endif
    }
}
