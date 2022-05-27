using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Abp.Domain.Entities;
using FclEx.Abp.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.ObjectMapping;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Abp.Domain.Dtos
{
    public class KeyValueDtoTests : AbpTests<AbpTestModule>
    {
        public KeyValueDtoTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void MapToDto_Test()
        {
            var mapper = ServiceProvider.GetRequiredService<IObjectMapper>();
            var entity = new KeyValueEntity
            {
                Id = 1,
                ObjectId = nameof(KeyValueDtoTests),
                Key = nameof(MapToDto_Test),
                Value = nameof(MapToDto_Test) + "_Value"
            };
            var dto = mapper.Map<KeyValueDto>(entity);

            //Assert.Equal(entity.Id, dto.Id);
            //Assert.Equal(entity.ObjectId, dto.ObjectId);
            //Assert.Equal(entity.Key, dto.Key);
            //Assert.Equal(entity.Value, dto.Value);

            AssertExt.EverySameNameMemberEqual(entity, dto);
        }

        [Fact]
        public void MapToEntity_Test()
        {
            var mapper = ServiceProvider.GetRequiredService<IObjectMapper>();
            var dto = new KeyValueDto
            {
                Id = 1,
                ObjectId = nameof(KeyValueDtoTests),
                Key = nameof(MapToEntity_Test),
                Value = nameof(MapToEntity_Test) + "_Value"
            };
            var entity = mapper.Map<KeyValueEntity>(dto);

            Assert.Equal(dto.Id, entity.Id);
            Assert.Equal(dto.ObjectId, entity.ObjectId);
            Assert.Equal(dto.Key, entity.Key);
            Assert.Equal(dto.Value, entity.Value);
        }
    }
}
