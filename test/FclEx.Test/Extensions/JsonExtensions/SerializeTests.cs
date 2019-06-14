using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Extensions;
using Newtonsoft.Json;
using Xunit;

namespace FclEx.Test.Extensions.JsonExtensions
{
    public class SerializeTests
    {
        [Fact]
        public void SerializeToJObject_Test()
        {
            var obj = new Tester();
            var jObject = obj.SerializeToJObject(new JsonOptions(formatting: Formatting.None));
            Assert.Equal("{\"Name\":\"Name\",\"Count\":1}", jObject.ToSimpleString());
        }

        [Fact]
        public void SerializeToJObjectCamel_Test()
        {
            var obj = new Tester();
            var jObject = obj.SerializeToJObjectCamel();
            Assert.Equal("{\"name\":\"Name\",\"Count\":1}", jObject.ToSimpleString());
        }
    }
}
