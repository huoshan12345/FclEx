using FclEx.Extensions;
using FclEx.Json.Converters;
using Newtonsoft.Json;
using Xunit;

namespace FclEx.Json
{
    public class ReadStringAsObjectConverterTests
    {
        private static string TestCase { get; } = "{\"username\":\"huoshan12345\",\"badge_count\":\"{\\\"seq_id\\\": 8, \\\"badge_count_at_ms\\\": 1573226541064}\"}";

        private class Tester
        {
            [JsonProperty("username")]
            public string UserName { get; set; }

            [JsonConverter(typeof(ReadStringAsObjectConverter))]
            [JsonProperty("badge_count")]
            public BadgeCount BadgeCount { get; set; }
        }

        private class TesterOfString
        {
            [JsonProperty("username")]
            public string UserName { get; set; }

            [JsonConverter(typeof(ReadStringAsObjectConverter))]
            [JsonProperty("badge_count")]
            public string BadgeCount { get; set; }
        }

        private class BadgeCount
        {
            [JsonProperty("seq_id")]
            public long SeqId { get; set; }

            [JsonProperty("badge_count_at_ms")]
            public long BadgeCountAtMs { get; set; }
        }

        [Fact]
        public void ReadObject_Test()
        {
            var obj = TestCase.ToJToken().ToObject<Tester>();
            Assert.NotNull(obj.BadgeCount);
            Assert.Equal(8, obj.BadgeCount.SeqId);
            Assert.Equal(1573226541064, obj.BadgeCount.BadgeCountAtMs);
        }


        [Fact]
        public void ReadString_Test()
        {
            var obj = TestCase.ToJToken().ToObject<TesterOfString>();
            var badgeCount = obj.BadgeCount.ToJToken().ToObject<BadgeCount>();
            Assert.Equal(8, badgeCount.SeqId);
            Assert.Equal(1573226541064, badgeCount.BadgeCountAtMs);
        }
    }
}
