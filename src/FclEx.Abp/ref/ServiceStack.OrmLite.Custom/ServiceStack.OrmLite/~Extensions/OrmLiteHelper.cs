using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ServiceStack.OrmLite
{
    public static partial class OrmLiteHelper
    {
        private static readonly ConcurrentDictionary<Type, List<string>> _fieldsDic = new();

        public static IList<string> GetAllFields<T>()
        {
            return _fieldsDic.GetOrAdd(typeof(T),
                t => ModelDefinition<T>.Definition.FieldDefinitions.ConvertAll(m => m.Name));
        }

        public static IList<string> GetAllFieldsExcept<T>(params Expression<Func<T, object>>[] exceptFields)
        {
            var all = GetAllFields<T>();
            if (exceptFields.IsEmpty()) return all;
            var ex = exceptFields.SelectMany(f => f.GetFieldNames());
            return all.Except(ex).ToArray();
        }

        public static FieldDefinition GetIdField<T>()
        {
            var fields = ModelDefinition<T>.Definition.FieldDefinitions;
            var key = fields.FirstOrDefault(m => m.IsPrimaryKey)
                      ?? fields.FirstOrDefault(m => m.Name.Equals("id", StringComparison.InvariantCultureIgnoreCase));
            if (key == null) throw new KeyNotFoundException("Can not find primary key field fot type: " + typeof(T).Name);
            return key;
        }

        public static FieldDefinition GetField<T>(string name)
        {
            return ModelDefinition<T>.Definition.FieldDefinitions.Single(m => m.Name == name);
        }

        public static FieldDefinition GetField<T>(Expression<Func<T, object>> selector)
        {
            var name = selector.GetFieldNames().Single();
            return GetField<T>(name);
        }
    }
}
