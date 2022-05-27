using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using FclEx.Abp.Domain.Entities;
using FclEx.Abp.Domain.Entities.Interfaces;
using FclEx.Abp.Orm;
using FclEx.Abp.Xunit;
using ServiceStack;
using ServiceStack.OrmLite;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Abp.OrmLite
{
    public class OrmLiteAttributeTests : AbpOrmLiteTest
    {
        public OrmLiteAttributeTests(ITestOutputHelper output, Action<AbpTestsOptions> action = null)
            : base(output, action)
        {
        }

        [Index(true, nameof(Value))]
        [Index(false, nameof(Key))]
        [Table("TableAttribute")]
        [ServiceStack.DataAnnotations.Alias("AliasAttribute")]
        public class Tester : IEntity
        {
            public int Id { get; set; }
            [Required] [MaxLength(64)] public string Key { get; set; }
            [Required] public string Value { get; set; }

            [Column("ColumnAttribute")]
            public string Column { get; set; }

            [Column("ColumnAttribute")]
            [ServiceStack.DataAnnotations.Alias("AliasAttribute")]
            public string ColumnAndAlias { get; set; }
        }

        [Fact]
        public void KeyValueEntity_Test()
        {
            var id = OrmLiteHelper.GetField<KeyValueEntity>(m => m.Id);
            Assert.True(id.IsPrimaryKey);
            Assert.True(id.AutoIncrement);

            var objectId = OrmLiteHelper.GetField<KeyValueEntity>(m => m.ObjectId);
            Assert.False(objectId.IsNullable);

            var key = OrmLiteHelper.GetField<KeyValueEntity>(m => m.Key);
            Assert.False(key.IsNullable);

            var value = OrmLiteHelper.GetField<KeyValueEntity>(m => m.Value);
            Assert.False(value.IsNullable);

            Assert.Equal(2, ModelDefinition<KeyValueEntity>.Definition.CompositeIndexes.Count);
            AssertIndexExist<KeyValueEntity>(false, m => m.ObjectId);
            AssertIndexExist<KeyValueEntity>(false, m => m.Key);
        }

        [Fact]
        public void Tester_Test()
        {
            Assert.Equal("AliasAttribute", ModelDefinition<Tester>.Definition.ModelName);

            var id = OrmLiteHelper.GetField<Tester>(m => m.Id);
            Assert.True(id.IsPrimaryKey);
            Assert.True(id.AutoIncrement);

            var key = OrmLiteHelper.GetField<Tester>(m => m.Key);
            Assert.False(key.IsNullable);

            var value = OrmLiteHelper.GetField<Tester>(m => m.Value);
            Assert.False(value.IsNullable);

            Assert.Equal(2, ModelDefinition<Tester>.Definition.CompositeIndexes.Count);
            AssertIndexExist<Tester>(false, m => m.Key);
            AssertIndexExist<Tester>(true, m => m.Value);

            var column = OrmLiteHelper.GetField<Tester>(m => m.Column);
            Assert.Equal("ColumnAttribute", column.FieldName);

            var columnAndAlias = OrmLiteHelper.GetField<Tester>(m => m.ColumnAndAlias);
            Assert.Equal("AliasAttribute", columnAndAlias.FieldName);
        }

        private static void AssertIndexExist<T>(bool isUnique, params string[] propertyNames)
        {
            var model = ModelDefinition<T>.Definition;
            Assert.Contains(model.CompositeIndexes,
                m => m.Unique == isUnique
                     && propertyNames.Length == m.FieldNames.Count
                     && propertyNames.All(x => m.FieldNames.Contains(x)));
        }

        private static void AssertIndexExist<T>(bool isUnique, Expression<Func<T, object>> selector)
        {
            var names = selector.GetFieldNames();
            AssertIndexExist<T>(isUnique, names);
        }
    }
}
