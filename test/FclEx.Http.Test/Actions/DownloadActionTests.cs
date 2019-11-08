using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FclEx.Http.Actions;
using FclEx.Http.Core;
using FclEx.Http.Services;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Http.Test.Actions
{
    public class DownloadActionTests
    {
        private readonly ITestOutputHelper _output;

        public DownloadActionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Download_Fail()
        {
            var action = new DownloadAction("http://127.0.0.1:1", HttpClientService.Default);
            var res = await action.ExecuteAutoAsync();
            Assert.True(res.IsError);
            _output.WriteLine(res.Exception.ToString());
        }
    }
}
