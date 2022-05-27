using System.Runtime.Serialization;
using System.Text;
using EasyCaching.Serialization.Json;
using EasyCaching.Serialization.MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyCaching.Core.Serialization
{
    public class StringAsRawSerializerTests
    {
        public class Tester
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        [Fact]
        public void Serialize_BinarySerializer_Test()
        {
            var stringAsRawSerializer = new StringAsRawEasyCachingSerializer(
                new DefaultBinaryFormatterSerializer(), Encoding.UTF8);

            var obj = new Tester { Age = 10, Name = "xxxxxxxxx" };
            Assert.Throws<SerializationException>(() => stringAsRawSerializer.SerializeObject(obj));
        }


        [Fact]
        public void Serialize_JsonSerializer_Test()
        {
            var ioc = new ServiceCollection()
                .AddEasyCaching(o => o.WithJson())
                .BuildServiceProvider();
            var jsonSerializer = ioc.GetRequiredService<IEasyCachingSerializer>();
            Assert.IsType<DefaultJsonSerializer>(jsonSerializer);
            var stringAsRawSerializer = new StringAsRawEasyCachingSerializer(jsonSerializer, Encoding.UTF8);

            var obj = new Tester { Age = 10, Name = "xxxxxxxxx" };
            var bytes = stringAsRawSerializer.SerializeObject(obj);

            var newObj = stringAsRawSerializer.DeserializeObject(bytes);
            Assert.IsType<Tester>(newObj);

            var newTypedObj = (Tester)newObj;
            Assert.Equal(obj.Name, newTypedObj.Name);
            Assert.Equal(obj.Age, newTypedObj.Age);
        }

        [Fact]
        public void Serialize_MessagePack_Test()
        {
            var provider = new ServiceCollection()
                .AddEasyCaching(o => o.WithMessagePack())
                .BuildServiceProvider();

            var jsonSerializer = provider.GetRequiredService<IEasyCachingSerializer>();
            Assert.IsType<DefaultMessagePackSerializer>(jsonSerializer);
            var stringAsRawSerializer = new StringAsRawEasyCachingSerializer(jsonSerializer, Encoding.UTF8);

            var obj = new Tester { Age = 10, Name = "xxxxxxxxx" };
            var bytes = stringAsRawSerializer.SerializeObject(obj);

            var newObj = stringAsRawSerializer.DeserializeObject(bytes);
            Assert.IsType<Tester>(newObj);

            var newTypedObj = (Tester)newObj;
            Assert.Equal(obj.Name, newTypedObj.Name);
            Assert.Equal(obj.Age, newTypedObj.Age);
        }
    }
}
