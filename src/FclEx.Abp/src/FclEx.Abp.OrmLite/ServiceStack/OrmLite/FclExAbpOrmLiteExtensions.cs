using System;
using FclEx.Abp.Domain.Entities.Interfaces;

namespace ServiceStack.OrmLite
{
    public static class FclExAbpOrmLiteExtensions
    {
        public static SqlExpression<TEntity> Undeleted<TEntity>(this SqlExpression<TEntity> sql) where TEntity : ISoftDelete
        {
            return sql.Where(m => !m.IsDeleted);
        }

        public static SqlExpression<TEntity> Active<TEntity>(this SqlExpression<TEntity> sql) where TEntity : IPassivable
        {
            return sql.Where(m => m.IsActive);
        }

        public static SqlExpression<TEntity> Valid<TEntity>(this SqlExpression<TEntity> sql) where TEntity : ISoftDelete, IPassivable
        {
            return sql.Undeleted().Active();
        }


    }
}
