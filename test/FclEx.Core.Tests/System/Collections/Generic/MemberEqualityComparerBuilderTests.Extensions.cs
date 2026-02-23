using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic;

partial class MemberEqualityComparerBuilderTests
{
    public record TestModel(int Id, bool IsDirectory, long Length, string Name, string? FullPath);

    [Fact]
    public void AddAllDataMembers_Test()
    {
        var comparer = MemberEqualityComparerBuilder<TestModel>
            .Create()
            .AddAllDataMembers(false, m => m.Id)
            .Build();

        var random = new Random(0);

        for (var i = 0; i < 1000; i++)
        {
            var path = random.NextBoolean() ? null : random.NextString(10);
            var x = new TestModel(random.Next(), random.NextBoolean(), random.NextInt64(), random.NextString(10), path);
            var y = ObjectHelper.CloneByJson(x);
            Assert.Equal(x, y);

            Assert.Equal(x, y, comparer);
            {
                var set = new HashSet<TestModel>([x], comparer);
                Assert.Contains(y, set);
            }
            {
                var z = y with { Id = y.Id + 1 };
                Assert.Equal(y, z, comparer);

                var set = new HashSet<TestModel>([y], comparer);
                Assert.Contains(z, set);
            }
            {
                var z = y with { IsDirectory = !y.IsDirectory };
                Assert.NotEqual(y, z, comparer);

                var set = new HashSet<TestModel>([y], comparer);
                Assert.DoesNotContain(z, set);
            }
            {
                var z = y with { Length = y.Length + 1 };
                Assert.NotEqual(y, z, comparer);

                var set = new HashSet<TestModel>([y], comparer);
                Assert.DoesNotContain(z, set);
            }
            {
                var z = y with { Name = y.Name + 1 };
                Assert.NotEqual(y, z, comparer);

                var set = new HashSet<TestModel>([y], comparer);
                Assert.DoesNotContain(z, set);
            }
            {
                var z = y with { FullPath = y.FullPath + 1 };
                Assert.NotEqual(y, z, comparer);

                var set = new HashSet<TestModel>([y], comparer);
                Assert.DoesNotContain(z, set);
            }
        }
    }
}
