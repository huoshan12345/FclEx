namespace System.Collections.Generic;

public class ReadOnlyListTests
{
    [Fact]
    public void ToString_PreservesNullElementPositions()
    {
        var list = new ReadOnlyList<string?>(["first", null, "last"]);

        Assert.Equal("[first, , last]", list.ToString());
    }
}
