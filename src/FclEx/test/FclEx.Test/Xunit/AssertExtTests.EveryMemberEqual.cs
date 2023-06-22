using System;
using System.Collections.Generic;
using System.Linq;
using FclEx;
using Xunit.Sdk;

namespace Xunit;

public partial class AssertExtTests
{
    public class Tester
    {
        public bool Bool { get; set; }
        public int Int { get; set; }
        public int? NullableInt { get; set; }
        public float Float { get; set; }
        public double? Double { get; set; }
        public decimal Decimal { get; set; }
        public string? String;
        public Tester? Child { get; set; }
        public List<Tester?>? List { get; set; }
    }

    public class Tester2
    {
        public Tester2? Child { get; set; }
    }

    private static Tester? CreateTester(bool nested, int level = 0)
    {
        if (level >= 3)
            return null;

        var random = new Random();
        var src = new Tester
        {
            Bool = random.NextBool(),
            Int = random.Next(),
            NullableInt = null,
            Float = random.NextDouble().CastTo<float>(),
            Double = random.NextDouble(),
            Decimal = random.NextDouble().CastTo<decimal>(),
            String = random.NextString(10),
            Child = null,
            List = null
        };
        if (nested)
        {
            src.Child = CreateTester(true, level + 1);
            src.List = Enumerable.Range(1, 10).Select(m => CreateTester(true, level + 1)).ToList();
        }
        return src;
    }

    [Fact]
    public void EveryMemberEqual_Success()
    {
        var src = CreateTester(false);
        var dest = src.DeepClone();
        AssertExt.EveryMemberEqual(src, dest);
    }

    [Fact]
    public void EveryMemberEqual_EmptyObject_Fail()
    {
        Assert.Throws<EqualException>(() => AssertExt.EveryMemberEqual(new object(), new object()));
    }

    [Fact]
    public void EveryMemberEqual_EmptyList_Fail()
    {
        AssertExt.EveryMemberEqual(new List<int>(), new List<int>());
    }

    [Fact]
    public void EveryMemberEqual_EmptyObject_Success()
    {
        Assert.Throws<EqualException>(() => AssertExt.EveryMemberEqual(new object(), new object()));
    }

    [Fact]
    public void EveryMemberEqual_CircularReference_Success()
    {
        var src = new Tester2();
        var dest = new Tester2();

        src.Child = src;
        dest.Child = dest;

        Assert.Throws<EqualException>(() => AssertExt.EveryMemberEqual(src, dest));

        dest.Child = src;
        AssertExt.EveryMemberEqual(src, dest);
    }

    [Fact]
    public void EveryMemberEqual_ExcludeMembers_Success()
    {
        var src = CreateTester(false);
        var dest = src.DeepClone();
        Assert.NotNull(dest);
        dest.Int++;

        Assert.Throws<EqualException>(() => AssertExt.EveryMemberEqual(src, dest));
        AssertExt.EveryMemberEqual(src, dest, nameof(dest.Int));
    }

    [Fact]
    public void EveryMemberEqual_Nested_Success()
    {
        var src = CreateTester(true);
        var dest = src.DeepClone();
        AssertExt.EveryMemberEqual(src, dest);
    }

    [Fact]
    public void EveryMemberEqual_Nested_ExcludeMembers_Success()
    {
        var src = CreateTester(true);
        {
            var dest = src.DeepClone();
            Assert.NotNull(dest);
            dest.Int++;
            Assert.Throws<EqualException>(() => AssertExt.EveryMemberEqual(src, dest));
            AssertExt.EveryMemberEqual(src, dest, nameof(dest.Int));
        }

        {
            var dest = src.DeepClone();
            Assert.NotNull(dest);
            dest.Int++;

            Assert.NotNull(dest.Child);
            dest.Child.Int++;

            Assert.Throws<EqualException>(() => AssertExt.EveryMemberEqual(src, dest));
            Assert.Throws<EqualException>(() => AssertExt.EveryMemberEqual(src, dest, "Int"));
            Assert.Throws<EqualException>(() => AssertExt.EveryMemberEqual(src, dest, "Child.Int"));
            AssertExt.EveryMemberEqual(src, dest, "Int", "Child.Int");
        }
    }
}