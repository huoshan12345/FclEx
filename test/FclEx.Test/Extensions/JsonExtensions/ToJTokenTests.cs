using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Extensions;
using Newtonsoft.Json;
using Xunit;

namespace FclEx.Test.Extensions.JsonExtensions
{
    public class ToJTokenTests
    {
        private class Tester
        {
            public string Name { get; set; }
            [JsonProperty("Count")]
            public int Count { get; set; }
        }

        [Fact]
        public void ToJTokenCamel_Test()
        {
            var obj = new Tester();
            var json = obj.ToJTokenCamel().ToString(Formatting.None);
            Assert.Equal("{\"name\":null,\"Count\":0}", json);
        }
    }
}
