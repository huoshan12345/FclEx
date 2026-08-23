namespace FclEx.Dapper;

[CollectionDefinition(nameof(DapperGlobalConfigurationCollection), DisableParallelization = true)]
public sealed class DapperGlobalConfigurationCollection;

[Collection(nameof(DapperGlobalConfigurationCollection))]
public class FclExDapperConfigurationTests
{
    [Fact]
    public void AccessingDapperHelper_DoesNotRegisterGuidTypeHandler()
    {
        var hadGuidTypeHandler = SqlMapper.HasTypeHandler(typeof(Guid));

        _ = DapperHelper.GetEntityDefinition(typeof(FirstEntity));

        Assert.Equal(hadGuidTypeHandler, SqlMapper.HasTypeHandler(typeof(Guid)));
    }

    [Fact]
    public void Apply_RegistersMapping_AndDisposeRestoresDefaultMapping()
    {
        ResetTypeMap<MappedEntity>();

        try
        {
            Assert.IsType<DefaultTypeMap>(SqlMapper.GetTypeMap(typeof(MappedEntity)));

            using (DapperHelper.CreateConfiguration().AddColumnMapping<MappedEntity>().Apply())
            {
                var map = SqlMapper.GetTypeMap(typeof(MappedEntity));

                Assert.IsNotType<DefaultTypeMap>(map);
                Assert.Equal(nameof(MappedEntity.Name), map.GetMember("stored_name")?.Property?.Name);
            }

            Assert.IsType<DefaultTypeMap>(SqlMapper.GetTypeMap(typeof(MappedEntity)));
        }
        finally
        {
            ResetTypeMap<MappedEntity>();
        }
    }

    [Fact]
    public void Apply_DefaultConflictBehavior_ThrowsBeforeApplyingAnyMapping()
    {
        ResetTypeMap<FirstEntity>();
        ResetTypeMap<ConflictingEntity>();
        var existingMap = CreateMap<ConflictingEntity>();
        SqlMapper.SetTypeMap(typeof(ConflictingEntity), existingMap);

        try
        {
            var builder = DapperHelper.CreateConfiguration()
                .AddColumnMappings(typeof(FirstEntity), typeof(ConflictingEntity));

            Assert.Throws<InvalidOperationException>(() => builder.Apply());
            Assert.IsType<DefaultTypeMap>(SqlMapper.GetTypeMap(typeof(FirstEntity)));
            Assert.Same(existingMap, SqlMapper.GetTypeMap(typeof(ConflictingEntity)));
        }
        finally
        {
            ResetTypeMap<FirstEntity>();
            ResetTypeMap<ConflictingEntity>();
        }
    }

    [Fact]
    public void Apply_ConflictBehavior_CanKeepOrTemporarilyReplaceExistingMapping()
    {
        ResetTypeMap<ConflictingEntity>();
        var existingMap = CreateMap<ConflictingEntity>();
        SqlMapper.SetTypeMap(typeof(ConflictingEntity), existingMap);
        var builder = DapperHelper.CreateConfiguration().AddColumnMapping<ConflictingEntity>();

        try
        {
            using (builder.Apply(DapperRegistrationConflictBehavior.KeepExisting))
                Assert.Same(existingMap, SqlMapper.GetTypeMap(typeof(ConflictingEntity)));

            using (builder.Apply(DapperRegistrationConflictBehavior.Replace))
                Assert.NotSame(existingMap, SqlMapper.GetTypeMap(typeof(ConflictingEntity)));

            Assert.Same(existingMap, SqlMapper.GetTypeMap(typeof(ConflictingEntity)));
        }
        finally
        {
            ResetTypeMap<ConflictingEntity>();
        }
    }

    [Fact]
    public void Apply_EquivalentRegistrations_AreReferenceCounted()
    {
        ResetTypeMap<FirstEntity>();

        try
        {
            var builder = DapperHelper.CreateConfiguration().AddColumnMapping<FirstEntity>();
            var first = builder.Apply();
            var appliedMap = SqlMapper.GetTypeMap(typeof(FirstEntity));
            var second = builder.Apply();

            Assert.Same(appliedMap, SqlMapper.GetTypeMap(typeof(FirstEntity)));

            first.Dispose();
            Assert.Same(appliedMap, SqlMapper.GetTypeMap(typeof(FirstEntity)));

            second.Dispose();
            Assert.IsType<DefaultTypeMap>(SqlMapper.GetTypeMap(typeof(FirstEntity)));
        }
        finally
        {
            ResetTypeMap<FirstEntity>();
        }
    }

    [Fact]
    public void AddColumnMappingsFromAssembly_OnlyRunsWhenExplicitlyRequested()
    {
        ResetTypeMap<AssemblyMappedEntity>();

        try
        {
            _ = DapperHelper.GetEntityDefinition(typeof(AssemblyMappedEntity));
            Assert.IsType<DefaultTypeMap>(SqlMapper.GetTypeMap(typeof(AssemblyMappedEntity)));

            using var registration = DapperHelper.CreateConfiguration()
                .AddColumnMappingsFromAssembly(typeof(AssemblyMappedEntity).Assembly)
                .Apply(DapperRegistrationConflictBehavior.KeepExisting);

            Assert.IsNotType<DefaultTypeMap>(SqlMapper.GetTypeMap(typeof(AssemblyMappedEntity)));
        }
        finally
        {
            ResetTypeMap<AssemblyMappedEntity>();
        }
    }

    private static CustomPropertyTypeMap CreateMap<TEntity>()
    {
        return new CustomPropertyTypeMap(typeof(TEntity), (_, _) => null!);
    }

    private static void ResetTypeMap<TEntity>()
    {
        SqlMapper.SetTypeMap(typeof(TEntity), null);
    }

    private sealed class FirstEntity
    {
        public int Id { get; set; }
    }

    private sealed class ConflictingEntity
    {
        public int Id { get; set; }
    }

    private sealed class MappedEntity
    {
        [Column("stored_name")]
        public string? Name { get; set; }
    }
}

[Table("assembly_mapped_entity")]
public sealed class AssemblyMappedEntity
{
    public int Id { get; set; }
}
