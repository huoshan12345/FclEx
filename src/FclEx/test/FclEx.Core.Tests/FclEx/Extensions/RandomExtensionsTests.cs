using FclEx.TestModels;

namespace FclEx.Extensions;

public class RandomExtensionsTests(ITestOutputHelper output)
{
    [Fact]
    public void NextBlittable_Struct_Test()
    {
        var random = new Random(0);
        for (var i = 0; i < 10; i++)
        {
            var x = random.NextBlittable<BlittableStruct>();
            Assert.Equal(4, x.Arr.Length);
            output.WriteLine(x);
        }
    }
}