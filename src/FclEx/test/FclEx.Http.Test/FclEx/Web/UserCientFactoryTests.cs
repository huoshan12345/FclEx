using FclEx.Http;
using FclEx.Http.Proxy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Web
{
    public class UserCientFactoryTests : WebTests
    {
        public UserCientFactoryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Resolve_Test()
        {
            var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient>>();
            Assert.IsType<UserClientFactory<TestUserClient>>(factory);
        }

        [Fact]
        public void Create_Test()
        {
            var account = new UserAccount("test", "test");
            var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient>>();
            var client = factory.Create(account);
            Assert.NotNull(client);
            Assert.Equal(client.Account, account);
        }

        [Fact]
        public void Create_WithProxy_Test()
        {
            var account = new UserAccount("test", "test");
            var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient>>();
            var client = factory.Create(account);
            Assert.Equal(WebProxyExt.None, client.HttpService.WebProxy);

            var proxy = WebProxyExt.Create("http://localhost:8888");
            client = factory.Create(account, proxy);
            Assert.Equal(proxy, client.HttpService.WebProxy);
        }
    }
}
