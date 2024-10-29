using FclEx.TestModels;

namespace FclEx.Extensions;

public class RandomExtensionsTests(ITestOutputHelper output)
{
    [Fact]
    public void Next_Struct_Test()
    {
        var random = new Random(0);
        for (var i = 0; i < 100; i++)
        {
            var x = random.Next<UnmanagedStruct>();
            Assert.Equal(4, x.Arr.Length);
            output.WriteLine(x);
        }
    }
}