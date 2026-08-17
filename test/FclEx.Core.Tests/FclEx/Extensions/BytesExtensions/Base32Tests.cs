namespace FclEx.Extensions.BytesExtensions;

public class Base32Tests
{
    public static readonly TheoryData<string, string> TestCases = new()
    {
        ("对任意字节数据进行编码的方案", "4WX3TZF3XPTIJD7FVWL6RCUC42K3BZUNV3UL7G7IUGGOPPEW46QIDZ42QTTJNOPGUGEA===="),
        ("GRLVQM2ULJDDGWCQKREUURBXIZLF", "I5JEYVSRJUZFKTCKIRCEOV2DKFFVERKVKVJEEWCJLJGEM==="),
    };

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

    [Theory]
    [InlineData("f", "MY======")]
    [InlineData("fo", "MZXQ====")]
    [InlineData("foo", "MZXW6===")]
    [InlineData("foob", "MZXW6YQ=")]
    [InlineData("fooba", "MZXW6YTB")]
    [InlineData("foobar", "MZXW6YTBOI======")]
    public void Base32ToBytes_Accepts_Canonical_Rfc4648_Encoding(string plain, string base32)
    {
        Assert.Equal(plain, base32.Base32ToBytes().GetString());
        Assert.Equal(plain, base32.TrimEnd('=').Base32ToBytes().GetString());
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AAA")]
    [InlineData("AAAAAA")]
    [InlineData("MY=====")]
    [InlineData("MY=====A")]
    [InlineData("========")]
    [InlineData("MZ======")]
    public void Base32ToBytes_Rejects_Invalid_Length_Padding_And_Tail_Bits(string base32)
    {
        Assert.Throws<ArgumentException>(() => base32.Base32ToBytes());
    }

    [Fact]
    public void Base32ToBytes_Rejects_Null()
    {
        string input = null!;

        Assert.Throws<ArgumentNullException>(() => input.Base32ToBytes());
    }
}
