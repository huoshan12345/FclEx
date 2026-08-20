namespace Xunit;

public partial class AssertEx
{
    extension(Assert)
    {
        public static void SkipIfInGithubAction()
        {
            if (TestHelper.IsGithubAction)
                Assert.Skip("Skipping test in GitHub Action.");
        }
    }
}
