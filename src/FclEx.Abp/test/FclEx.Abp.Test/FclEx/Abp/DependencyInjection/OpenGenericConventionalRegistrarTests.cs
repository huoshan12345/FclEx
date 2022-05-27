using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Abp.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Abp.DependencyInjection
{
    public class OpenGenericConventionalRegistrarTests : AbpTests<AbpTestModule>
    {
        public interface IGenericSingleton<out T> : ISingletonDependency{}
        public class GenericSingleton<T> : IGenericSingleton<T>{}

        public interface IGenericTransient<T> : ITransientDependency { }
        public class GenericTransient<T> : IGenericTransient<T> { }

        public OpenGenericConventionalRegistrarTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GenericSingleton_Test()
        {
            var objInt = ServiceProvider.GetRequiredService<IGenericSingleton<int>>();
            var objInt2 = ServiceProvider.GetRequiredService<IGenericSingleton<int>>();
            Assert.Equal(objInt, objInt2);

            var objStr = ServiceProvider.GetRequiredService<IGenericSingleton<string>>();
            var objStr2 = ServiceProvider.GetRequiredService<IGenericSingleton<string>>();
            Assert.Equal(objStr, objStr2);
        }

        [Fact]
        public void GenericTransient_Test()
        {
            var objInt = ServiceProvider.GetRequiredService<IGenericTransient<int>>();
            var objInt2 = ServiceProvider.GetRequiredService<IGenericTransient<int>>();
            Assert.NotEqual(objInt, objInt2);

            var objStr = ServiceProvider.GetRequiredService<IGenericTransient<string>>();
            var objStr2 = ServiceProvider.GetRequiredService<IGenericTransient<string>>();
            Assert.NotEqual(objStr, objStr2);
        }
    }
}
