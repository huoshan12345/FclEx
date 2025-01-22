using Xunit.Sdk;

namespace Xunit;

partial class AssertExTests
{
    public class TestModel
    {
        public bool Bool { get; set; }
        public int Int { get; set; }
        public int? NullableInt { get; set; }
        public float Float { get; set; }
        public double? Double { get; set; }
        public decimal Decimal { get; set; }
        public string? String { get; set; }
        public TestModel? Child { get; set; }
        public List<TestModel?>? List { get; set; }
    }

    public class TestModel2
    {
        public TestModel2? Child { get; set; }
    }

    private static TestModel? CreateTestModel(bool nested, int level = 0)
    {
        if (level >= 3)
            return null;

        var random = new Random();
        var src = new TestModel
        {
            Bool = random.NextBoolean(),
            Int = random.Next(),
            NullableInt = null,
            Float = random.NextDouble().CastTo<float>(),
            Double = random.NextDouble(),
            Decimal = random.NextDouble().CastTo<decimal>(),
            String = random.NextString(10),
            Child = null,
            List = null,
        };
        if (nested)
        {
            src.Child = CreateTestModel(true, level + 1);
            src.List = Enumerable.Range(1, 10).Select(m => CreateTestModel(true, level + 1)).ToList();
        }
        return src;
    }

    [Fact]
    public void EveryMemberEqual_Success()
    {
        var src = CreateTestModel(false);
        var dest = src.CloneByJson();
        AssertEx.EveryMemberEqual(src, dest);
    }

    [Fact]
    public void EveryMemberEqual_EmptyObject_Fail()
    {
        Assert.Throws<EqualException>(() => AssertEx.EveryMemberEqual(new object(), new object()));
    }

    [Fact]
    public void EveryMemberEqual_EmptyList_Fail()
    {
        AssertEx.EveryMemberEqual(new List<int>(), new List<int>());
    }

    [Fact]
    public void EveryMemberEqual_EmptyObject_Success()
    {
        Assert.Throws<EqualException>(() => AssertEx.EveryMemberEqual(new object(), new object()));
    }

    [Fact]
    public void EveryMemberEqual_CircularReference_Success()
    {
        var src = new TestModel2();
        var dest = new TestModel2();

        src.Child = src;
        dest.Child = dest;

        Assert.Throws<EqualException>(() => AssertEx.EveryMemberEqual(src, dest));

        dest.Child = src;
        AssertEx.EveryMemberEqual(src, dest);
    }

    [Fact]
    public void EveryMemberEqual_ExcludeMembers_Success()
    {
        var src = CreateTestModel(false);
        var dest = src.CloneByJson();
        Assert.NotNull(dest);
        dest.Int++;

        Assert.Throws<EqualException>(() => AssertEx.EveryMemberEqual(src, dest));
        AssertEx.EveryMemberEqual(src, dest, nameof(dest.Int));
    }

    [Fact]
    public void EveryMemberEqual_Nested_Success()
    {
        var src = CreateTestModel(true);
        var dest = src.CloneByJson();
        AssertEx.EveryMemberEqual(src, dest);
    }

    [Fact]
    public void EveryMemberEqual_Nested_ExcludeMembers_Success()
    {
        var src = CreateTestModel(true);
        {
            var dest = src.CloneByJson();
            Assert.NotNull(dest);
            dest.Int++;
            Assert.Throws<EqualException>(() => AssertEx.EveryMemberEqual(src, dest));
            AssertEx.EveryMemberEqual(src, dest, nameof(dest.Int));
        }

        {
            var dest = src.CloneByJson();
            Assert.NotNull(dest);
            dest.Int++;

            Assert.NotNull(dest.Child);
            dest.Child.Int++;

            Assert.Throws<EqualException>(() => AssertEx.EveryMemberEqual(src, dest));
            Assert.Throws<EqualException>(() => AssertEx.EveryMemberEqual(src, dest, "Int"));
            Assert.Throws<EqualException>(() => AssertEx.EveryMemberEqual(src, dest, "Child.Int"));
            AssertEx.EveryMemberEqual(src, dest, "Int", "Child.Int");
        }
    }
}