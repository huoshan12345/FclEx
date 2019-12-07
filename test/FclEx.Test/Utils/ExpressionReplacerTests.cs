using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FclEx.Utils;
using Xunit;

namespace FclEx.Test.Utils
{
    public class ExpressionReplacerTests
    {
        [Fact]
        public void ReplaceParameters_Test()
        {
            Expression<Func<IEnumerable<int>, int, bool>> filter = (l, x) => x % 2 == 0;
            var list = Enumerable.Range(1, 100).ToList();
            var items = Filter(list, filter).ToList();
            Assert.Equal(list.Where(x => x % 2 == 0), items);
        }

        [Fact]
        public void ReplaceParameters_Mutil_Times_Test()
        {
            Expression<Func<IEnumerable<int>, int, bool>> filter = (l, x) => x % 2 == 0;

            var list = Enumerable.Range(1, 100).ToList();
            IEnumerable<int> p = list;
            for (var i = 0; i < 10; ++i)
            {
                p = Filter(p, filter);
            }
            Assert.Equal(list.Where(x => x % 2 == 0), p);
        }

        [Fact]
        public async Task ReplaceParameters_Mutil_Tasks_Test()
        {
            Expression<Func<IEnumerable<int>, int, bool>> filter = (l, x) => x % 2 == 0;

            var list = Enumerable.Range(1, 100).ToList();
            var tasks = Enumerable.Range(1, 10000).Select(m =>
                (Func<Task<List<int>>>)(() => Task.Run(() => Filter(list, filter).ToList())));
            var results = await tasks.Select(m => m()).WhenAll();
            var expected = list.Where(x => x % 2 == 0).ToList();

            Assert.Equal(10000, results.Length);
            foreach (var result in results)
                Assert.Equal(expected, result);
        }


        private static IEnumerable<T> Filter<T>(IEnumerable<T> enumerable, Expression<Func<IEnumerable<T>, T, bool>> filter)
        {
            var para = Expression.Constant(enumerable);
            var paras = filter.Parameters;
            var newExp = ExpressionReplacer.Replace(filter.Body, paras.First(), para);
            var newParas = paras.Skip(1);
            var newFilter = Expression.Lambda<Func<T, bool>>(newExp, newParas);
            return enumerable.Where(newFilter.Compile());
        }
    }
}
