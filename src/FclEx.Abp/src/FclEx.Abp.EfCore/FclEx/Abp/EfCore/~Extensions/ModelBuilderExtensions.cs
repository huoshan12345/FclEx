namespace FclEx.Abp.EfCore;

public static class ModelBuilderExtensions
{
    public static ModelBuilder SetFclExAbpAttributes(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            modelBuilder.SetFclExAbpAttributes(entity);
        }
        return modelBuilder;
    }

    public static ModelBuilder SetFclExAbpAttributes(this ModelBuilder modelBuilder, IMutableEntityType type)
    {
        var e = modelBuilder.Entity(type.Name);
        var table = type.ClrType.GetCustomAttribute<TableAttribute>();
        if (table == null)
        {
            e.ToTable(type.ClrType.Name.TrimEnd("Entity"));
        }

        var indexes = type.ClrType.GetCustomAttributes<Orm.IndexAttribute>();
        foreach (var index in indexes)
        {
            e.HasIndex(index.PropertyNames)
                .IsUnique(index.IsUnique);
        }

        var entityType = type.ClrType.GetImplementedInterface(typeof(IEntity<>));
        if (entityType != null)
        {
            e.HasKey(EntityMemberNames.Id);
            if (entityType.GenericTypeArguments[0].IsInteger())
            {
                e.Property(EntityMemberNames.Id).ValueGeneratedOnAdd()
                    .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            }
        }

        return modelBuilder;
    }
}