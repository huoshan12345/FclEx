using System.Threading.Tasks;
using FclEx.Utils;
using Xunit;

namespace FclEx.Actions
{
    public partial class ExtensionsTests
    {
        [Fact]
        public async Task Union_Success_Test()
        {
            var (successful, _, result, _) = await CommonAction.Create(() => Task.FromResult(1))
                .Union(r => CommonAction.Create(() => Task.FromResult(1 + r)))
                .Union((a, b) => CommonAction.Create(() => Task.FromResult(1 + a + b)))
                .ExecuteAsync();

            Assert.True(successful);
            Assert.Equal((1, 2, 4), result);
        }

        [Fact]
        public async Task Union_Error_Begin_Test()
        {
            var flag = false;
            var (successful, _, _, ex) = await CommonAction.Create(() => OperateResult.CreateError<int>("error"))
                .Union(r => CommonAction.Create(() =>
                {
                    flag = true;
                    return Task.FromResult(1 + r);
                }))
                .ExecuteAsync();

            Assert.False(flag);
            Assert.False(successful);
            Assert.Equal("error", ex.Message);
        }


        [Fact]
        public async Task Union_Error_Middle_Test()
        {
            var flag = false;
            var (successful, _, _, ex) = await CommonAction.Create(() => Task.FromResult(1))
                .Union(r =>
                {
                    Assert.Equal(1, r);
                    return CommonAction.Create(() => OperateResult.CreateError<int>("error"));
                })
                .Union((a, b) =>
                {
                    flag = true;
                    return CommonAction.Create(() => Task.FromResult(1 + a + b));
                })
                .ExecuteAsync();

            Assert.False(flag);
            Assert.False(successful);
            Assert.Equal("error", ex.Message);
        }

        [Fact]
        public async Task Union_Error_End_Test()
        {
            var (successful, _, _, ex) = await CommonAction.Create(() => Task.FromResult(1))
                .Union(r => CommonAction.Create(() => Task.FromResult(1 + r)))
                .Union((a, b) =>
                {
                    Assert.Equal(1, a);
                    Assert.Equal(2, b);
                    return CommonAction.Create(() => OperateResult.CreateError<int>("error"));
                })
                .ExecuteAsync();

            Assert.False(successful);
            Assert.Equal("error", ex.Message);
        }

        [Fact]
        public async Task Union_Errors_Test()
        {
            var (successful, _, _, ex) = await CommonAction.Create(() => OperateResult.CreateError<int>("error1"))
                .Union(r => CommonAction.Create(() => OperateResult.CreateError<int>("error2")))
                .ExecuteAsync();

            Assert.False(successful);
            Assert.Equal("error1", ex.Message);
        }
    }
}
