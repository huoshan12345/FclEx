namespace FclEx.Extensions.BytesExtensions;

public class Base32Tests
{
    public static readonly IEnumerable<object[]> TestCases = new[]
    {
        ("对任意字节数据进行编码的方案", "4WX3TZF3XPTIJD7FVWL6RCUC42K3BZUNV3UL7G7IUGGOPPEW46QIDZ42QTTJNOPGUGEA===="),
        ("GRLVQM2ULJDDGWCQKREUURBXIZLF", "I5JEYVSRJUZFKTCKIRCEOV2DKFFVERKVKVJEEWCJLJGEM==="),
    }.Select(m => new object[] { m.Item1, m.Item2 });

    [Theory]
    [MemberData(nameof(TestCases))]
    public void ToBase32_Test(string plain, string base32)
    {
        var b32 = plain.ToBytes().ToBase32();
        Assert.Equal(base32, b32);
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void ToBytesFromBase32_Test(string plain, string base32)
    {
        var p = base32.Base32ToBytes().GetString();
        Assert.Equal(p, plain);
    }
}