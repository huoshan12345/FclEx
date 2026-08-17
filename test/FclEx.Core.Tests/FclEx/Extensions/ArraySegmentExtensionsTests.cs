namespace FclEx.Extensions;

public class ArraySegmentExtensionsTests
{
    [Fact]
    public void Slice_Uses_Offsets_Relative_To_The_Parent_Segment()
    {
        var segment = new ArraySegment<int>([0, 1, 2, 3, 4], 1, 3);

        var slice = segment.Slice(1, 2);

        Assert.Equal(2, slice.Offset);
        Assert.Equal([2, 3], slice.ToArray());
    }

    [Fact]
    public void Slice_Cannot_Exceed_The_Parent_Segment()
    {
        var segment = new ArraySegment<int>(new int[10], 2, 2);

        Assert.Throws<ArgumentOutOfRangeException>(() => segment.Slice(0, 3));
    }
}
