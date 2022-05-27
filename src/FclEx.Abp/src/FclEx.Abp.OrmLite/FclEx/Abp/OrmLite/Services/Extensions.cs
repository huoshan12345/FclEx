using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FclEx.Abp.Domain.Services;
using FclEx.Extensions;
using ServiceStack.OrmLite;

namespace FclEx.Abp.OrmLite.Services
{
    public static class Extensions
    {
        public static async Task<T?> GetByIdAsync<T>(this IEntityService service, object id)
        {
            var where = OrmLiteHelper.BuildFilterById<T>(id);
            var list = await service.GetListAsync(where, 1).DonotCapture();
            return list.FirstOrDefault();
        }

        public static async Task DeleteByIdAsync<T>(this IEntityService service, object id)
        {
            var where = OrmLiteHelper.BuildFilterById<T>(id);
            await service.DeleteListAsync(where).DonotCapture();
        }
    }
}
