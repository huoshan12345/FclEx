using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FclEx.Abp.Domain.Services;
using FclEx.Extensions;
using ServiceStack.OrmLite;

namespace FclEx.Abp.OrmLite.Services
{
    public abstract class OrmLiteEntityService : IEntityService
    {
        protected readonly IOrmLiteConStrResolver _conStrResolver;
        protected readonly string _conName;

        protected OrmLiteEntityService(IOrmLiteConStrResolver conStrResolver, string conName)
        {
            _conStrResolver = conStrResolver;
            _conName = conName;
        }

        public async Task<List<T>> GetListAsync<T>(Expression<Func<T, bool>> filter, int? max)
        {
            using var con = await _conStrResolver.OpenAsync(_conName).DonotCapture();
            var list = await con.From<T>()
                .Where(filter)
                .Take(max)
                .ToListAsync(con);
            return list;
        }

        public async Task InsertListAsync<T>(IEnumerable<T> list)
        {
            using var con = await _conStrResolver.OpenAsync(_conName).DonotCapture();
            await con.InsertBulkAsync(list);
        }

        public async Task UpdateAsync<T>(T entity)
        {
            using var con = await _conStrResolver.OpenAsync(_conName).DonotCapture();
            await con.UpdateAsync(entity);
        }

        public async Task DeleteListAsync<T>(Expression<Func<T, bool>> filter)
        {
            using var con = await _conStrResolver.OpenAsync(_conName).DonotCapture();
            await con.DeleteAsync(filter);
        }
    }
}
