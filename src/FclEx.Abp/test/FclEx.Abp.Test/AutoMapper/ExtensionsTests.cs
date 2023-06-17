using Xunit;

namespace AutoMapper
{
    public class ExtensionsTests
    {
        public class A
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Tags { get; set; }
        }

        public class B
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string?[]? Tags { get; set; }
        }

        [Fact]
        public void MapStrToArray_Test()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<A, B>().MapStrToArray(m => m.Tags, m => m.Tags, ","));
            var mapper = config.CreateMapper();
            var a = new A
            {
                Id = 1,
                Name = nameof(A.Name),
                Tags = new[] { nameof(A.Id), nameof(A.Name), nameof(A.Tags) }.JoinWith(",")
            };
            var b = mapper.Map<B>(a);
            Assert.Equal(a.Id, b.Id);
            Assert.Equal(a.Name, b.Name);
            Assert.NotNull(b.Tags);
            Assert.Equal(a.Tags, b.Tags.JoinWith(","));
        }

        [Fact]
        public void MapStrToArray_Null_Test()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<A, B>().MapStrToArray(m => m.Tags, m => m.Tags, ","));
            var mapper = config.CreateMapper();
            var a = new A
            {
                Id = 1,
                Name = nameof(A.Name),
                Tags = null
            };
            var b = mapper.Map<B>(a);
            Assert.Equal(a.Id, b.Id);
            Assert.Equal(a.Name, b.Name);
            Assert.NotNull(b.Tags);
            Assert.Empty(b.Tags);
        }

        [Fact]
        public void MapArrayToStr_Test()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<B, A>().MapArrayToStr(m => m.Tags, m => m.Tags, ","));
            var mapper = config.CreateMapper();
            var b = new B
            {
                Id = 1,
                Name = nameof(A.Name),
                Tags = new[] { nameof(A.Id), nameof(A.Name), nameof(A.Tags) }
            };
            var a = mapper.Map<A>(b);
            Assert.Equal(b.Id, a.Id);
            Assert.Equal(b.Name, a.Name);
            Assert.Equal(b.Tags.JoinWith(","), a.Tags);
        }

        [Fact]
        public void MapArrayToStr_Null_Test()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<B, A>().MapArrayToStr(m => m.Tags, m => m.Tags, ","));
            var mapper = config.CreateMapper();
            var b = new B
            {
                Id = 1,
                Name = nameof(A.Name),
                Tags = null
            };
            var a = mapper.Map<A>(b);
            Assert.Equal(b.Id, a.Id);
            Assert.Equal(b.Name, a.Name);
            Assert.Equal(string.Empty, a.Tags);
        }
    }
}
