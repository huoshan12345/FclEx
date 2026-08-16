namespace FclEx.Helpers;

public class LambdaHelperTests
{
    [Fact]
    public void GetPropertyLambdaExp_ShouldRetainEntryForTheLifetimeOfItsType()
    {
        var first = GetNameSelector();

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

        var second = GetNameSelector();

        Assert.Same(first, second);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static LambdaExpression GetNameSelector()
    {
        return LambdaHelper.GetPropertyLambdaExp<Model>(nameof(Model.Name));
    }

    private sealed class Model
    {
        public string Name { get; set; } = "";
    }
}
