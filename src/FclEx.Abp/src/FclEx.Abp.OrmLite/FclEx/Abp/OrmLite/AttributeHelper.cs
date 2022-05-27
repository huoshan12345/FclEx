using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using FclEx.Abp.Domain.Entities.Interfaces;
using FclEx.Abp.Orm;
using FclEx.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using Volo.Abp.Reflection;

namespace FclEx.Abp.OrmLite
{
    public static class AttributeHelper
    {
        public static void AddOrmLiteAttributeForAllEntityTypes(this IServiceProvider provider)
        {
            var typeFinder = provider.GetRequiredService<ITypeFinder>();
            foreach (var type in typeFinder.GetEntityTypes())
            {
                AddOrmLiteAttribute(type);
            }
        }

        public static void HandleTableAttribute(Type type)
        {
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            if (tableAttr != null)
            {
                if (tableAttr.Schema.IsValid() && !type.HasAttribute<ServiceStack.DataAnnotations.SchemaAttribute>())
                {
                    type.AddAttributes(new ServiceStack.DataAnnotations.SchemaAttribute(tableAttr.Schema));
                }
                if (tableAttr.Name.IsValid() && !type.HasAttribute<ServiceStack.DataAnnotations.AliasAttribute>())
                {
                    type.AddAttributes(new ServiceStack.DataAnnotations.AliasAttribute(tableAttr.Name));
                }
            }
        }

        public static void HandleAutoRenameAttribute(Type type)
        {
            if (type.HasAttribute<ServiceStack.DataAnnotations.AliasAttribute>())
                return;

            var attribute = type.GetCustomAttribute<AutoRenameAttribute>();
            if (attribute == null || attribute.RemoveEntityPostfix)
            {
                type.AddAttributes(new ServiceStack.DataAnnotations.AliasAttribute(type.Name.TrimEnd("Entity")));
            }
        }

        public static void HandleIndexAttribute(Type type)
        {
            var indexAttrs = type.GetCustomAttributes<IndexAttribute>(true).ToArray();
            if (indexAttrs.Any())
            {
                var newAttrs = indexAttrs.Select(m => new ServiceStack.DataAnnotations.CompositeIndexAttribute
                {
                    FieldNames = m.PropertyNames.ToList(),
                    Unique = m.IsUnique
                }).Cast<Attribute>().ToArray();
                type.AddAttributes(newAttrs);
            }
        }

        public static void AddOrmLiteAttribute(Type type)
        {
            HandleTableAttribute(type);
            HandleAutoRenameAttribute(type);
            HandleIndexAttribute(type);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                var propType = prop.PropertyType.UnwarpNullable();
                if (prop.Name == nameof(IEntity<long>.Id) || prop.GetCustomAttribute<KeyAttribute>() != null)
                {
                    prop.ReplaceAttribute(new ServiceStack.DataAnnotations.PrimaryKeyAttribute());
                    if (prop.PropertyType.IsInteger())
                    {
                        prop.ReplaceAttribute(new ServiceStack.DataAnnotations.AutoIncrementAttribute());
                    }
                }
                var requiredAttr = prop.GetCustomAttribute<RequiredAttribute>(true);
                if (requiredAttr != null)
                {
                    prop.ReplaceAttribute(new RequiredAttribute());
                }
                var maxLength = prop.GetCustomAttribute<MaxLengthAttribute>();
                if (maxLength != null)
                {
                    prop.ReplaceAttribute(new StringLengthAttribute(maxLength.Length));
                }
                if (propType.IsEnum)
                {
                    prop.ReplaceAttribute(new ServiceStack.DataAnnotations.EnumAsIntAttribute());
                }
                if (!prop.HasAttribute<ServiceStack.DataAnnotations.AliasAttribute>())
                {
                    var column = prop.GetCustomAttribute<ColumnAttribute>();
                    if (column != null)
                    {
                        prop.ReplaceAttribute(new ServiceStack.DataAnnotations.AliasAttribute(column.Name));
                    }
                }
                if (propType.IsClass
                    && propType != typeof(string)
                    && !prop.HasAttribute<ServiceStack.DataAnnotations.ReferenceAttribute>())
                {
                    prop.ReplaceAttribute(new ServiceStack.DataAnnotations.IgnoreAttribute());
                }
            }
        }
    }
}
