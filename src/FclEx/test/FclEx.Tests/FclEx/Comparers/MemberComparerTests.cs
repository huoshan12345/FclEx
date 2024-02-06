namespace FclEx.Comparers;

public class MemberComparerTests
{
    [Fact]
    public void Test()
    {
        foreach (var num in Enumerable.Range(100, 5))
        {
            var random = new Random(num);
            var list = Enumerable.Range(1, 10000).Select(m => random.NextBoolean(20) ? null : new Person
            {
                Age = random.Next(1, 100),
                Name = random.NextString(10),
                Height = random.Next(100, 200),
            }).ToList();

            var orderedList = list
                .OrderBy(m => m?.Age)
                .ThenBy(m => m?.Name)
                .ThenByDescending(m => m?.Height)
                .ToList();

            var cmp = MemberComparerBuilder<Person>
                .Create(m => m.Age)
                .OrderBy(m => m.Name)
                .OrderBy(m => m.Height, true)
                .CreateComparison();
            list.Sort(cmp);

            Assert.True(orderedList.SequenceEqual(orderedList));
        }

    }
}