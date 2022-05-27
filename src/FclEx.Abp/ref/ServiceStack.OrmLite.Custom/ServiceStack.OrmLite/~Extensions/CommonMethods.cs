using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using FclEx;
using FclEx.Extensions;

namespace ServiceStack.OrmLite
{
    public static class CommonMethods
    {
        internal static MethodInfo ContainsOfEnumerable { get; } = typeof(Enumerable).GetMethods()
            .Where(m => m.Name == nameof(Enumerable.Contains))
            .Where(m => m.IsGenericMethodDefinition)
            .Single(m => m.GetParameters().Length == 2);

        internal static MethodInfo ContainsOfEnumerableOfObj { get; } = ContainsOfEnumerable.MakeGenericMethod(typeof(object));

        internal static MethodInfo MaxOfSql { get; } = typeof(Sql).GetMethods()
            .Where(m => m.Name == nameof(Sql.Max))
            .Single(m => m.IsGenericMethodDefinition);

        internal static MethodInfo MaxOfSqlOfObj { get; } = MaxOfSql.MakeGenericMethod(typeof(object));

        internal static MethodInfo ContainsOfString { get; } = typeof(string).GetMethods()
            .Where(m => m.Name == nameof(string.Contains))
            .Single(m => m.GetParameters() is { Length: 1 } paras && paras[0].ParameterType == typeof(string));

        internal static MethodInfo StartsWith { get; } = typeof(string).GetMethods()
            .Where(m => m.Name == nameof(string.StartsWith))
            .Single(m => m.GetParameters() is { Length: 1 } paras && paras[0].ParameterType == typeof(string));

        private static object[] TakeMethodParasOfSingle { get; } = { 1 };

        private static readonly ConcurrentDictionary<Type, MethodInfo> _takeMethods = new();

        private static MethodInfo GetMethodOfTake(Type type)
        {
            return _takeMethods.GetOrAdd(type, k => k.GetMethods().Single(m =>
                m.Name == nameof(SqlExpression<object>.Take)
                && m.GetParameters().Length == 1));
        }

        internal static ISqlExpression InvokeTakeOne(ISqlExpression exp)
        {
            var type = exp.GetType();
            if (type.IsGenericType && type.IsSubclassOfRawGeneric(typeof(SqlExpression<>)))
            {
                var method = GetMethodOfTake(type);
                return (ISqlExpression)method.Invoke(exp, TakeMethodParasOfSingle);
            }
            if (exp is IUntypedSqlExpression ue)
            {
                return ue.Take(1);
            }
            throw new NotSupportedException("cannot call take of type: " + type.ShortName());
        }

        internal static MethodInfo DeleteById { get; } = typeof(OrmLiteWriteApi)
            .GetMethod(nameof(OrmLiteWriteApi.DeleteById), new[] { typeof(IDbConnection), typeof(object), typeof(Action<IDbCommand>) });

        internal static MethodInfo DeleteByIdAsync { get; } = typeof(OrmLiteWriteApiAsync)
            .GetMethod(nameof(OrmLiteWriteApiAsync.DeleteByIdAsync), new[] { typeof(IDbConnection), typeof(object), typeof(Action<IDbCommand>), typeof(CancellationToken) });

        internal static MethodInfo InsertAsync { get; } = typeof(OrmLiteWriteApiAsync).GetMethods()
            .Where(m => m.Name == nameof(OrmLiteWriteApiAsync.InsertAsync))
            .Where(m =>
            {
                var paras = m.GetParameters();
                return paras.Length == 5
                       && paras[0].ParameterType == typeof(IDbConnection)
                       && paras[1].ParameterType.IsGenericParameter
                       && paras[2].ParameterType == typeof(bool)
                       && paras[3].ParameterType == typeof(bool)
                       && paras[4].ParameterType == typeof(CancellationToken);
            }).Single();

        internal static MethodInfo Insert { get; } = typeof(OrmLiteWriteApi).GetMethods()
            .Where(m => m.Name == nameof(OrmLiteWriteApi.Insert))
            .Where(m =>
            {
                var paras = m.GetParameters();
                return paras.Length == 4
                       && paras[0].ParameterType == typeof(IDbConnection)
                       && paras[1].ParameterType.IsGenericParameter
                       && paras[2].ParameterType == typeof(bool)
                       && paras[3].ParameterType == typeof(bool);
            }).Single();

    }
}
