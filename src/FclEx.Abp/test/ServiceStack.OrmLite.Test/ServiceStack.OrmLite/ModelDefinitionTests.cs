using System.Reflection.Emit;
using Xunit;

namespace ServiceStack.OrmLite
{
    public class ModelDefinitionTests
    {
        [Fact]
        public void Quota_Test()
        {
            var props = new[]
            {
                ("Name", typeof(string)),
                ("Age", typeof(int)),
                ("Status", typeof(byte)),
                ("CreateTime", typeof(DateTimeOffset)),
            };
            for (var i = 0; i < LicenseUtils.FreeQuotas.OrmLiteTables + 1; i++)
            {
                var newType = TypeBuilderHelper.CreateType("Person_" + i, props);
                var def = newType.GetModelMetadata();
                foreach (var (name, type) in props)
                {
                    var field = def.FieldDefinitions.FirstOrDefault(m => m.FieldName == name);
                    Assert.NotNull(field);
                    Assert.Equal(type, field.FieldType);
                }
            }
        }
    }
}
