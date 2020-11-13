using System;
using System.Net;
using Xunit;

namespace FclEx.Http.Proxy
{
    public class WebProxyExtTests
    {
        [Theory]
        [InlineData("userName", "password")]
        [InlineData("user@Name", "pass@word")]
        public void Create_WithAuthUri(string userName, string password)
        {
            var uriBuilder = new UriBuilder("http://192.168.1.221:8888")
            {
                UserName = userName.UrlEncode(),
                Password = password.UrlEncode()
            };

            var proxy = WebProxyExt.Create(uriBuilder.Uri);
            var auth = (NetworkCredential)proxy.Credentials;
            Assert.Equal(userName, auth.UserName);
            Assert.Equal(password, auth.Password);
        }
    }
}
