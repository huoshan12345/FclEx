using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Xunit;

namespace FclEx.Test.Extensions.JsonExtensions
{
    public class ToJsonTests
    {
        [Fact]
        public void ToJsonCamel_Test()
        {
            var obj = new Tester();
            var json = obj.ToJsonCamel();
            Assert.Equal("{\"name\":\"Name\",\"Count\":1}", json);
        }

        [Fact]
        public void DateTimeToJsonCamel_Test()
        {
            var obj = new DateTimeTester() { DateTime = new DateTime(2019, 1, 2, 3, 4, 5, DateTimeKind.Unspecified) };
            var json = obj.ToJsonCamel();
            Assert.Equal("{\"name\":null,\"dateTime\":\"2019-01-02T03:04:05+08:00\"}", json);
        }

        [Fact]
        public void GetSettings_SameOptions_SameResult()
        {
            var options = new JsonOptions();
            var settings = FclEx.JsonExtensions.GetSettings(options);
            var settings2 = FclEx.JsonExtensions.GetSettings(options);
            Assert.Same(settings, settings2);
        }

        [Fact]
        public void GetSettings_EquatableOptions_SameResult()
        {
            var settings = FclEx.JsonExtensions.GetSettings(new JsonOptions(Formatting.Indented));
            var settings2 = FclEx.JsonExtensions.GetSettings(new JsonOptions(Formatting.Indented));
            Assert.Same(settings, settings2);
        }

        [Fact]
        public void GetSettings_NonEquatableOptions_DifferentResult()
        {
            var settings = FclEx.JsonExtensions.GetSettings(new JsonOptions(Formatting.Indented));
            var settings2 = FclEx.JsonExtensions.GetSettings(new JsonOptions(Formatting.None));
            Assert.NotSame(settings, settings2);
        }
    }
}
