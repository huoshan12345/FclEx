namespace FclEx.Utils;

public class JsonStringSerializerTests
{
    [Fact]
    public void Deserialize_ShouldAcceptJsonWithSurroundingWhitespace()
    {
        var serializer = JsonStringSerializer.Instance;

        Assert.Equal(123, serializer.Deserialize<int>(" \r\n123\t "));
        Assert.Equal([1, 2], serializer.Deserialize<int[]>(" \r\n[1,2]\t ")!);
    }
}
