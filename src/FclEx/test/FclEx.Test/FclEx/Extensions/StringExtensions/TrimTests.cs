#nullable enable
using System;
using Xunit;

namespace FclEx.Extensions.StringExtensions
{
    public record TrimCase(string? Source, string? Trim, string? Result);

    public class TrimTests
    {
        public static readonly IEnumerable<object?[]> TrimStartCases = new TrimCase[]
        {
            new(null, null, null),
            new(null, "", null),
            new("", null, ""),
            new("aa_xx", "aa", "_xx"),
            new("aaaa_xx", "aa", "_xx"),
            new("aaaaa_xx", "aa", "a_xx"),
            new("aaaaaa_xx", "aa", "_xx"),
            new("aa_xx", "_", "aa_xx"),
            new("aa_xx", "", "aa_xx"),
            new("aa_xx", "xx", "aa_xx"),
        }.Select(m => new object?[] { m.Source, m.Trim, m.Result });

        [Theory]
        [MemberData(nameof(TrimStartCases))]
        public void TrimStart_Test(string source, string trim, string result)
        {
            var actual = source.TrimStart(trim);
            Assert.Equal(result, actual);
        }

        public static readonly IEnumerable<object?[]> TrimEndCases = new TrimCase[]
        {
            new(null, null, null),
            new(null, "", null),
            new("", null, ""),
            new("aa_xx", "xx", "aa_"),
            new("aa_xxx", "xx", "aa_x"),
            new("aa_xxxx", "xx", "aa_"),
            new("aa_xxxxxx", "xx", "aa_"),
            new("aa_xx", "_", "aa_xx"),
            new("aa_xx", "", "aa_xx"),
            new("aa_xx", "aa", "aa_xx"),
        }.Select(m => new object?[] { m.Source, m.Trim, m.Result });

        [Theory]
        [MemberData(nameof(TrimEndCases))]
        public void TrimEnd_Test(string source, string trim, string result)
        {
            var actual = source.TrimEnd(trim);
            Assert.Equal(result, actual);
        }

        private const string Base64ImgPrefix = "data:image/png;base64";
        private const string Base64ImgContent = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";
        private const string Base64Img = Base64ImgPrefix + "," + Base64ImgContent;

        [Fact]
        public void TrimStart_Contains_Test()
        {
            var source = "Bearer token";
            var result = source.TrimStart("Bearer ");
            Assert.Equal("token", result);
        }

        [Fact]
        public void TrimStart_DoesNotContain_Test()
        {
            var source = "Basic token";
            var result = source.TrimStart("Bearer ");
            Assert.Equal("Basic token", result);
        }

        [Fact]
        public void TrimStart_Null_Source_Test()
        {
            string? source = null;
            var result = source.TrimStart("Bearer ");
            Assert.Null(result);
        }

        [Fact]
        public void TrimStart_Null_TrimString_Test()
        {
            var source = "Basic token";
            var result = source.TrimStart(null);
            Assert.Equal(source, result);
        }

        [Fact]
        public void SkipUntil_DoesNotContainsSeparator()
        {
            const string text = "data";
            var result = text.SkipUntil(",");
            Assert.Equal(text, result);
        }

        [Fact]
        public void SkipUntil_SkipSeparator()
        {
            var base64 = Base64Img.SkipUntil(",");
            Assert.Equal(Base64ImgContent, base64);
        }

        [Fact]
        public void SkipUntil_DonotSkipSeparator()
        {
            var base64 = Base64Img.SkipUntil(",", false);
            Assert.StartsWith(",", base64);
            Assert.Equal(Base64ImgContent, base64.TrimStart(','));
        }

        [Fact]
        public void TakeUntil_DoesNotContainsSeparator()
        {
            const string text = "data";
            var result = text.TakeUntil(",");
            Assert.Equal(text, result);
        }

        [Fact]
        public void TakeUntil_IncludeSeparator()
        {
            var base64 = Base64Img.TakeUntil(",");
            Assert.Equal(Base64ImgPrefix + ",", base64);
        }

        [Fact]
        public void TakeUntil_DonotIncludeSeparator()
        {
            var base64 = Base64Img.TakeUntil(",", false);
            Assert.Equal(Base64ImgPrefix, base64);
        }
    }
}
