using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FclEx.Extensions;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace FclEx.Abp.OrmLite
{
    public static class Extensions
    {
        public static IDbConnectionFactory ResolveFac(this IOrmLiteConStrResolver resolver, string name)
        {
            return resolver.Get(name).ConFac;
        }

        public static void Do(this OrmLiteConStr conStr, Action<IDbConnection> action)
        {
            conStr.ConFac.Do(action);
        }

        public static T Do<T>(this OrmLiteConStr conStr, Func<IDbConnection, T> action)
        {
            return conStr.ConFac.Do(action);
        }

        public static Task DoAsync(this OrmLiteConStr conStr, Func<IDbConnection, Task> action)
        {
            return conStr.ConFac.DoAsync(action);
        }

        public static Task<T> DoAsync<T>(this OrmLiteConStr conStr, Func<IDbConnection, Task<T>> action)
        {
            return conStr.ConFac.DoAsync(action);
        }

        public static IDbConnection Open(this IOrmLiteConStrResolver resolver, string name)
        {
            return resolver.ResolveFac(name).OpenDbConnection();
        }

        public static Task<IDbConnection> OpenAsync(this IOrmLiteConStrResolver resolver, string name)
        {
            return resolver.ResolveFac(name).OpenAsync();
        }

        public static void EnsureDbCreated(this IOrmLiteConStrResolver resolver, string name)
        {
            resolver.Get(name).EnsureDbCreated();
        }

        public static void EnsureDbCreated(this OrmLiteConStr conStr)
        {
            if (!conStr.Provider.IfDatabaseExists(conStr.Str))
                conStr.Provider.CreateDatabase(conStr.Str);
        }

        public static void EnsureTableCreated<T>(this OrmLiteConStr conStr, bool overwrite = false)
        {
            conStr.EnsureTableCreated(typeof(T), overwrite);
        }

        public static void EnsureTableCreated(this OrmLiteConStr conStr, Type type, bool overwrite = false)
        {
            conStr.EnsureTableCreated(new[] { type }, overwrite);
        }

        public static void EnsureTableCreated(this OrmLiteConStr conStr, IEnumerable<Type> entityTypes, bool overwrite = false)
        {
            conStr.ConFac.Do(c =>
            {
                foreach (var type in entityTypes)
                    c.CreateTable(overwrite, type);
            });
        }

        public static void ValidateTables(this OrmLiteConStr conStr, IEnumerable<Type> entityTypes)
        {
            conStr.ConFac.Do(c => ValidateTables(c, entityTypes));
        }

        public static Task ValidateTablesAsync(this OrmLiteConStr conStr, IEnumerable<Type> entityTypes)
        {
            return conStr.ConFac.DoAsync(c => ValidateTablesAsync(c, entityTypes));
        }

        public static async Task ValidateTablesAsync(this IDbConnection con, IEnumerable<Type> entityTypes)
        {
            foreach (var type in entityTypes)
            {
                var e = type.CreateTestEntity();
                con.CreateTable(false, type);
                var id = await con.InsertObjectAsync(e, true);
                await con.DeleteByIdAsync(type, id);
            }
        }

        public static void ValidateTables(this IDbConnection con, IEnumerable<Type> entityTypes)
        {
            foreach (var type in entityTypes)
            {
                var e = type.CreateTestEntity();
                con.CreateTable(false, type);
                var id = con.InsertObject(e, true);
                con.DeleteById(type, id);
            }
        }

        public static object CreateTestEntity(this Type type, string defaultValue = "test")
        {
            var obj = type.CreateObject();
            var model = (ModelDefinition)typeof(ModelDefinition<>).MakeGenericType(type).InvokeMember(nameof(ModelDefinition<object>.Definition),
                BindingFlags.GetProperty, null, null, null)!;

            foreach (var field in model.FieldDefinitions)
            {
                if (!field.IsNullable)
                {
                    if (field.FieldType == typeof(string))
                    {
                        field.SetValueFn.Invoke(obj, defaultValue);
                    }
                    else if (field.FieldType == typeof(DateTime))
                    {
                        field.SetValueFn.Invoke(obj, DateTime.UtcNow);
                    }
                    else if (field.FieldType == typeof(DateTimeOffset))
                    {
                        field.SetValueFn.Invoke(obj, DateTimeOffset.UtcNow);
                    }
                }
            }
            return obj;
        }
    }
}
