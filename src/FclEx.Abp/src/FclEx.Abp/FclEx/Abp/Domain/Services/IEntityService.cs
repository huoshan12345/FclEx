using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Abp.Domain.Services
{
    public interface IEntityService
    {
        Task<List<T>> GetListAsync<T>(Expression<Func<T, bool>> filter, int? max = null);
        Task InsertListAsync<T>(IEnumerable<T> list);
        Task UpdateAsync<T>(T entity);
        Task DeleteListAsync<T>(Expression<Func<T, bool>> filter);
    }
}
