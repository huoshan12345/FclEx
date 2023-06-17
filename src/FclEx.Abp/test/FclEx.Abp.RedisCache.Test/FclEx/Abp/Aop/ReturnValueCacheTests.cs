using AspectCore.DynamicProxy;
using FclEx.Abp.RedisCache;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Abp.Aop
{
    public class ReturnValueCacheTests : AbpRedisTests
    {
        public const int CacheMaxMilliseconds = 800;
        public const int SleepMilliseconds = 1000;

        public static IEnumerable<object[]> Numbers { get; } = new[] { -1, 0, 1, 10 }
            .Select(m => new object[] { m }).ToArray();

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

        public ReturnValueCacheTests(ITestOutputHelper output)
            : base(output, o => o.AddTransient<IService, Service>())
        {
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
        public void IsStatic_SameObject_Test(int no)
        {
            var service = ServiceProvider.GetRequiredService<IService>();
            var itemFromStatic = service.GetStatic(no);

            var (_, tempItem, _, t) = Operate.Execute(() => service.GetStatic(no));
            Assert.NotNull(tempItem);
            Assert.Equal(itemFromStatic.Id, tempItem.Id);
            Assert.True(t.TotalMilliseconds < CacheMaxMilliseconds, t.TotalSeconds.ToString());
        }

        [Theory]
        [MemberData(nameof(Numbers))]
        public void IsStatic_DiffObject_Test(int no)
        {
            var service = ServiceProvider.GetRequiredService<IService>();
            var itemFromStatic = service.GetStatic(no);

            var tempService = ServiceProvider.GetRequiredService<IService>();
            var (_, tempitemFromStatic, _, t) = Operate.Execute(() => tempService.GetStatic(no));
            Assert.NotNull(tempitemFromStatic);
            Assert.Equal(itemFromStatic.Id, tempitemFromStatic.Id);
            Assert.True(t.TotalMilliseconds < CacheMaxMilliseconds, t.TotalSeconds.ToString());
        }


        [Theory]
        [MemberData(nameof(Numbers))]
        public void NotStatic_SameObject_Test(int no)
        {
            var service = ServiceProvider.GetRequiredService<IService>();
            var itemFromInstace = service.Get(no);

            var (_, tempItem, _, t) = Operate.Execute(() => service.Get(no));
            Assert.NotNull(tempItem);
            Assert.Equal(itemFromInstace.Id, tempItem.Id);
            Assert.True(t.TotalMilliseconds < CacheMaxMilliseconds, t.TotalSeconds.ToString());
        }

        [Theory]
        [MemberData(nameof(Numbers))]
        public void NotStatic_DiffObject_Test(int no)
        {
            var service = ServiceProvider.GetRequiredService<IService>();
            var itemFromInstace = service.Get(no);

            var tempService = ServiceProvider.GetRequiredService<IService>();
            var (_, tempItemFromInstace, _, t) = Operate.Execute(() => tempService.Get(no));
            Assert.NotNull(tempItemFromInstace);
            Assert.NotEqual(itemFromInstace.Id, tempItemFromInstace.Id);
            Assert.Equal($"{tempService.Id}_{no}", tempItemFromInstace.Id);
            Assert.True(t.TotalMilliseconds > SleepMilliseconds, t.TotalSeconds.ToString());
        }
    }
}
