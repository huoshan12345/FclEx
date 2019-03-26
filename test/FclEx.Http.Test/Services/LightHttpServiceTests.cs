using FclEx.Http.Proxy;
using FclEx.Http.Services;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Http.Test.Services
{
    public class LightHttpServiceTests
    {
        private readonly ITestOutputHelper _output;

        public LightHttpServiceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("http://localhost:1080")]
        public void Constructor_Test(string proxy)
        {
            var http = new HttpWebRequestService(proxy);
            Assert.Equal(WebProxyExt.Create(proxy), http.WebProxy);
        }
    }
}
