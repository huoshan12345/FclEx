namespace FclEx.Extensions;

public class CrossJoinTests
{
    [Fact]
    public void CrossJoin_MaterializesAOneShotRightSequenceOnce()
    {
        var enumerationCount = 0;

        IEnumerable<int> Right()
        {
            if (++enumerationCount > 1)
                throw new InvalidOperationException("The right sequence was enumerated more than once.");

            yield return 3;
            yield return 4;
        }

        var result = new[] { 1, 2 }.CrossJoin(Right()).ToArray();

        Assert.Equal([(1, 3), (1, 4), (2, 3), (2, 4)], result);
        Assert.Equal(1, enumerationCount);
    }
}
