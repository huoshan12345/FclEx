using System;
using System.Linq;

namespace FclEx.Comparers;

public class MemberComparerTests
{
    private static string CreateRondomString(int length, Random random)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[length];
        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }
        return new string(stringChars);
    }

    [Fact]
    public void Test()
    {
        foreach (var num in Enumerable.Range(100, 5))
        {
            var random = new Random(num);
            var list = Enumerable.Range(1, 10000).Select(m => random.NextBool(20) ? null : new Person
            {
                Age = random.Next(1, 100),
                Name = CreateRondomString(10, random),
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