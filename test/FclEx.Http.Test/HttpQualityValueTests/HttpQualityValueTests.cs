using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FclEx.Http.Core;
using Xunit;

namespace FclEx.Http.Test.HttpQualityValueTests
{
    public class HttpQualityValueTests
    {
        //  When no "q=" is specified, it defaults to 1
        public static string[] AcceptEncodings { get; } =
        {
            "gzip,deflate",
            "deflate,gzip",
            "gzip;q=.5,deflate",
            "gzip;q=0,deflate",
            "deflate;q=0.5,gzip;q=0.5,identity",
            "*"
        };
        public static string[] PreferOrder { get; } = { "gzip", "deflate" };
        public static string[] ExpectedEncoding { get; } =
        {
            "gzip",
            "gzip",
            "deflate",
            "deflate",
            null,
            "gzip",
        };

        public static IEnumerable<object[]> Cases { get; } = AcceptEncodings.Zip(ExpectedEncoding, (a, e) => (a, e)).Select(m => new object[] { m.a, m.e });

        [Theory]
        [MemberData(nameof(Cases))]
        public void Test(string acceptEncoding, string expectedEncoding)
        {
            var encodings = new HttpQualityValueList(acceptEncoding);
            var preferred = encodings.FindPreferred(PreferOrder);
            Assert.Equal(expectedEncoding, preferred.Name);
        }
    }
}
