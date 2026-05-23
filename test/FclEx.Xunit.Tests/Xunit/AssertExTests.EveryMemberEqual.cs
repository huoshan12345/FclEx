// CS8620 here is caused by known bug: https://github.com/dotnet/roslyn/issues/80024#issuecomment-3594618986
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

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
        var dest = ObjectHelper.CloneByJson(src);
        Assert.MembersEqual(src, dest);
    }

    [Fact]
    public void EveryMemberEqual_EmptyObject_Fail()
    {
        Assert.Throws<EqualException>(() => Assert.MembersEqual(new object(), new object()));
    }

    [Fact]
    public void EveryMemberEqual_EmptyList_Fail()
    {
        Assert.MembersEqual(new List<int>(), []);
    }

    [Fact]
    public void EveryMemberEqual_EmptyObject_Success()
    {
        Assert.Throws<EqualException>(() => Assert.MembersEqual(new object(), new object()));
    }

    [Fact]
    public void EveryMemberEqual_CircularReference_Success()
    {
        var src = new TestModel2();
        var dest = new TestModel2();

        src.Child = src;
        dest.Child = dest;

        Assert.Throws<EqualException>(() => Assert.MembersEqual(src, dest));

        dest.Child = src;
        Assert.MembersEqual(src, dest);
    }

    [Fact]
    public void EveryMemberEqual_ExcludeMembers_Success()
    {
        var src = CreateTestModel(false);
        var dest = ObjectHelper.CloneByJson(src);
        Assert.NotNull(dest);
        dest.Int++;

        Assert.Throws<EqualException>(() => Assert.MembersEqual(src, dest));
        Assert.MembersEqual(src, dest, nameof(dest.Int));
    }

    [Fact]
    public void EveryMemberEqual_Nested_Success()
    {
        var src = CreateTestModel(true);
        var dest = ObjectHelper.CloneByJson(src);
        Assert.MembersEqual(src, dest);
    }

    [Fact]
    public void EveryMemberEqual_Nested_ExcludeMembers_Success()
    {
        var src = CreateTestModel(true);
        {
            var dest = ObjectHelper.CloneByJson(src);
            Assert.NotNull(dest);
            dest.Int++;
            Assert.Throws<EqualException>(() => Assert.MembersEqual(src, dest));
            Assert.MembersEqual(src, dest, nameof(dest.Int));
        }

        {
            var dest = ObjectHelper.CloneByJson(src);
            Assert.NotNull(dest);
            dest.Int++;

            Assert.NotNull(dest.Child);
            dest.Child.Int++;

            Assert.Throws<EqualException>(() => Assert.MembersEqual(src, dest));
            Assert.Throws<EqualException>(() => Assert.MembersEqual(src, dest, "Int"));
            Assert.Throws<EqualException>(() => Assert.MembersEqual(src, dest, "Child.Int"));
            Assert.MembersEqual(src, dest, "Int", "Child.Int");
        }
    }
}