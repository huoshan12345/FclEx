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

        private class DateTimeTester
        {
            public string Name { get; set; }
            public DateTime DateTime { get; set; }
        }

        [Fact]
        public void ToJsonCamel_Test()
        {
            var obj = new Tester();
            var json = obj.ToJsonCamel();
            Assert.Equal("{\"name\":null,\"Count\":0}", json);
        }

        [Fact]
        public void DateTimeToJsonCamel_Test()
        {
            var obj = new DateTimeTester() { DateTime = new DateTime(2019, 1, 2, 3, 4, 5, DateTimeKind.Unspecified) };
            var json = obj.ToJsonCamel();
            Assert.Equal("{\"name\":null,\"dateTime\":\"2019-01-02 03:04:05\"}", json);
        }
    }
}
