using FclEx.Http.Proxy;
using FclEx.Web.Core;
using FclEx.Web.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Http.Test.Web
{
    public class UserCientFactoryTests : WebTests
    {
        public UserCientFactoryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Resolve_Test()
        {
            var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient, UserAccount>>();
            Assert.IsType<UserClientFactory<TestUserClient, UserAccount>>(factory);
        }

        [Fact]
        public void Create_Test()
        {
            var account = new UserAccount("test", "test");
            var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient, UserAccount>>();
            var client = factory.Create(account);
            Assert.NotNull(client);
            Assert.Equal(client.Account, account);
        }

        [Fact]
        public void Create_WithProxy_Test()
        {
            var account = new UserAccount("test", "test");
            var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient, UserAccount>>();
            var client = factory.Create(account);
            Assert.Equal(WebProxyExt.None, client.HttpService.WebProxy);

            var proxy = WebProxyExt.Create("http://localhost:8888");
            client = factory.Create(account, proxy);
            Assert.Equal(proxy, client.HttpService.WebProxy);
        }
    }
}
