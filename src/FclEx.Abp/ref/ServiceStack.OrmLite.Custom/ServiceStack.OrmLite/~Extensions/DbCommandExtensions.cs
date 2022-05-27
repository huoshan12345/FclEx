using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FclEx;
using FclEx.Extensions;
using MoreLinq;

namespace ServiceStack.OrmLite
{
    internal static partial class DbCommandExtensions
    {
        internal static IDbDataParameter[] CreateBulkInsertParas<TEntity>(this IDbCommand cmd,
            int index, TEntity e, ICollection<FieldDefinition> insertFields)
        {
            var provider = cmd.GetDialectProvider();
            var ps = insertFields.Select(x =>
            {
                var para = cmd.CreateParameter();
                provider.SetParameter(x, para);
                para.ParameterName += "_" + index;

                if (x.AutoId && x.FieldType == typeof(Guid))
                {
                    para.Value = Guid.NewGuid();
                    x.SetValueFn(e, para.Value);
                }
                else
                {
                    var value = x.GetValue(e);
                    var finalValue = provider.GetFieldValue(x, value) ?? DBNull.Value;
                    para.Value = finalValue;
                }

                return para;
            }).ToArray();
            return ps;
        }

        internal static void SetBulkInsertCmd<TEntity>(this IDbCommand cmd, IEnumerable<TEntity> entities)
        {
            var provider = cmd.GetDialectProvider();
            var modelDef = ModelDefinition<TEntity>.Definition;
            var tableName = provider.GetQuotedTableName(modelDef);
            var insertFields = modelDef.FieldDefinitionsArray.Where(m => !m.ShouldSkipInsert()).ToList();

            var fieldNames = insertFields
                .Select(m => provider.GetQuotedColumnName(m.FieldName))
                .JoinWith(",");

            var paraWithValues = entities
                .Select((m, i) => (Entity: m, Index: i, Paras: CreateBulkInsertParas(cmd, i, m, insertFields)))
                .ToList();

            var valuesStr = paraWithValues
                .Select(m => m.Paras
                    .Select(x => x.ParameterName)
                    .JoinWith(","))
                .Select(m => $"({m})")
                .JoinWith(",\n");
            var sql = $"INSERT INTO {tableName} ({fieldNames}) VALUES \n{valuesStr}";

            var paras = paraWithValues
                .SelectMany(m => m.Paras)
                .ToList();

            cmd.Parameters.Clear();
            cmd.Parameters.AddRange(paras);
            cmd.CommandText = sql;
        }
    }
}
