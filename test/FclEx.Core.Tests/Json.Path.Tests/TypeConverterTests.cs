using System.ComponentModel;

namespace Json.Path.Tests
{
    public class TypeConverterTests
    {

        [Fact]
        public void ConvertFromString()
        {
            var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPath));
            var path = typeConverter.ConvertFromInvariantString("$.store.book[*].author") as JsonPath;

            Assert.NotNull(path);
            Assert.Equal("$.store.book[*].author", path!.ToString());
        }

        [Fact]
        public void ConvertToString()
        {
            var path = JsonPath.Parse("$.store.book[*].author");
            var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPath));
            var pathString = typeConverter.ConvertToInvariantString(path);

            Assert.Equal("$.store.book[*].author", pathString);
        }

        [Fact]
        public void ConvertFromJsonPath()
        {
            var path = JsonPath.Parse("$.store.book[*].author");
            var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPath));
            var path2 = typeConverter.ConvertFrom(path) as JsonPath;

            Assert.NotNull(path2);
            Assert.NotSame(path, path2);
            Assert.Equal("$.store.book[*].author", path2!.ToString());
        }

        [Fact]
        public void ConvertToJsonPath()
        {
            var path = JsonPath.Parse("$.store.book[*].author");
            var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPath));
            var path2 = typeConverter.ConvertTo(path, typeof(JsonPath)) as JsonPath;

            Assert.NotNull(path2);
            Assert.NotSame(path, path2);
            Assert.Equal("$.store.book[*].author", path2!.ToString());
        }

    }
}
