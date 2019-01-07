using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Xunit;

namespace FclEx.Test.Extensions.JsonExtensions
{
    public class ToJsonTests
    {
        private class Tester
        {
            public string Name { get; set; }
            [JsonProperty("Count")]
            public int Count { get; set; }
        }

        [Fact]
        public void ToJsonCamel_Test()
        {
            var obj = new Tester();
            var json = obj.ToJsonCamel();
            Assert.Equal("{\"name\":null,\"Count\":0}", json);
        }
    }
}
