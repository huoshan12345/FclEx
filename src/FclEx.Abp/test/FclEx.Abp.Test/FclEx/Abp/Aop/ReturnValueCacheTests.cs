using AspectCore.DynamicProxy;
using FclEx.Abp.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Abp.Aop
{
    public class ReturnValueCacheTests : AbpAopTests<AbpTestModule>
    {
        public const int CacheMaxMilliseconds = 100;
        public const int SleepMilliseconds = 200;

        public static IEnumerable<object[]> Numbers { get; } = new[] { -1, 0, 1, 10 }
            .Select(m => new object[] { m }).ToArray();

        public ReturnValueCacheTests(ITestOutputHelper output)
            : base(output, o => o.AddTransient<IService, Service>())
        {
        }

        public class Model
        {
            public string Id { get; }
            public Model(string id) { Id = id; }
        }

        public interface IService
        {
            int Id { get; }

            [ReturnValueCache(IsStatic = true)]
            Model GetStatic(int id);

            [ReturnValueCache]
            Model Get(int id);
        }

        public class Service : IService
        {
            private static int _id = short.MinValue;
            public int Id { get; }

            public Service()
            {
                Id = Interlocked.Increment(ref _id);
            }

            public Model GetStatic(int id)
            {
                Thread.Sleep(SleepMilliseconds);
                return new Model(id.ToString());
            }

            public Model Get(int id)
            {
                Thread.Sleep(SleepMilliseconds);
                return new Model($"{_id}_{id}");
            }
        }

        [Fact]
        public void Aop_Test()
        {
            var service = ServiceProvider.GetRequiredService<IService>();
            Assert.IsNotType<Service>(service);
            Assert.True(service.IsProxy());
        }

        [Theory]
        [MemberData(nameof(Numbers))]
        public void TestSingle(int no)
        {
            var service = ServiceProvider.GetRequiredService<IService>();
            var itemFromStatic = service.GetStatic(no);
            var itemFromInstace = service.Get(no);

            for (var i = 0; i < 3; i++)
            {
                var (_, tempItem, _, t) = Operate.Execute(() => service.Get(no));
                Assert.NotNull(tempItem);
                Assert.Equal(itemFromInstace.Id, tempItem.Id);
                Assert.True(t.TotalMilliseconds < CacheMaxMilliseconds);
            }
            for (var i = 0; i < 3; i++)
            {
                var (_, tempItem, _, t) = Operate.Execute(() => service.GetStatic(no));
                Assert.NotNull(tempItem);
                Assert.Equal(itemFromStatic.Id, tempItem.Id);
                Assert.True(t.TotalMilliseconds < CacheMaxMilliseconds);
            }
        }

        [Theory]
        [MemberData(nameof(Numbers))]
        public void TestMultiple(int no)
        {
            var service = ServiceProvider.GetRequiredService<IService>();
            var itemFromStatic = service.GetStatic(no);
            for (var i = 0; i < 3; i++)
            {
                var tempService = ServiceProvider.GetRequiredService<IService>();
                var (_, tempitemFromStatic, _, timeFromStatic) = Operate.Execute(() => tempService.GetStatic(no));
                var (_, tempItemFromInstace, _, timeFromInstace) = Operate.Execute(() => tempService.Get(no));

                Assert.NotNull(tempitemFromStatic);
                Assert.Equal(itemFromStatic.Id, tempitemFromStatic.Id);

                Assert.NotNull(tempItemFromInstace);
                Assert.Equal($"{tempService.Id}_{no}", tempItemFromInstace.Id);

                Assert.True(timeFromStatic.TotalMilliseconds < CacheMaxMilliseconds, timeFromStatic.TotalMilliseconds.ToString());
                Assert.True(timeFromInstace.TotalMilliseconds > SleepMilliseconds, timeFromInstace.TotalMilliseconds.ToString());
            }
        }
    }
}
