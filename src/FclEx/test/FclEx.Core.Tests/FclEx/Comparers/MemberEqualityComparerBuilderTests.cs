namespace FclEx.Comparers;

public class MemberEqualityComparerBuilderTests
{
    public record Model(int Id, bool IsDirectory, long Length, string Name, string? FullPath);

    [Fact]
    public void AddAllProperties_Test()
    {
        var comparer = MemberEqualityComparerBuilder<Model>
            .Create()
            .AddAllDataMembers(false, m => m.Id)
            .Build();

        var random = new Random(0);

        for (var i = 0; i < 1000; i++)
        {
            var path = random.NextBoolean() ? null : random.NextString(10);
            var x = new Model(random.Next(), random.NextBoolean(), random.NextInt64(), random.NextString(10), path);
            var y = x.CloneByJson();
            Assert.Equal(x, y);

            Assert.Equal(x, y, comparer);
            {
                var set = x.Yield().ToHashSet(comparer);
                Assert.Contains(y, set);
            }
            {
                var z = y with { Id = y.Id + 1 };
                Assert.Equal(y, z, comparer);

                var set = y.Yield().ToHashSet(comparer);
                Assert.Contains(z, set);
            }
            {
                var z = y with { IsDirectory = !y.IsDirectory };
                Assert.NotEqual(y, z, comparer);

                var set = y.Yield().ToHashSet(comparer);
                Assert.DoesNotContain(z, set);
            }
            {
                var z = y with { Length = y.Length + 1 };
                Assert.NotEqual(y, z, comparer);

                var set = y.Yield().ToHashSet(comparer);
                Assert.DoesNotContain(z, set);
            }
            {
                var z = y with { Name = y.Name + 1 };
                Assert.NotEqual(y, z, comparer);

                var set = y.Yield().ToHashSet(comparer);
                Assert.DoesNotContain(z, set);
            }
            {
                var z = y with { FullPath = y.FullPath + 1 };
                Assert.NotEqual(y, z, comparer);

                var set = y.Yield().ToHashSet(comparer);
                Assert.DoesNotContain(z, set);
            }
        }
    }
}