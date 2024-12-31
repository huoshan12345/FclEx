using FclEx.Abp.Domain;
using FclEx.Abp.Orm;

namespace FclEx.Abp.EfCore;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyOrmAttributes(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            modelBuilder.ApplyOrmAttributes(entity);
        }
        return modelBuilder;
    }

    public static ModelBuilder ApplyOrmAttributes(this ModelBuilder modelBuilder, IMutableEntityType type)
    {
        var clrType = type.ClrType;
        var entity = modelBuilder.Entity(type.Name);
        var table = clrType.GetCustomAttribute<TableAttribute>();
        if (table == null)
        {
            var removeEntity = clrType.GetCustomAttribute<AutoRenameAttribute>()?.RemoveEntitySuffix ?? true;
            var name = removeEntity
                ? clrType.Name.TrimEnd("Entity")
                : clrType.Name;
            entity.ToTable(name);
        }

        var indexes = clrType.GetCustomAttributes<Orm.IndexAttribute>();
        foreach (var index in indexes)
        {
            entity.HasIndex(index.PropertyNames)
                .IsUnique(index.IsUnique);
        }

        var entityType = clrType.GetImplementedInterface(typeof(IEntity<>));
        if (entityType != null)
        {
            entity.HasKey(nameof(IEntity<int>.Id));
            if (entityType.GenericTypeArguments[0].IsInteger())
            {
                entity.Property(nameof(IEntity<int>.Id)).ValueGeneratedOnAdd()
                    .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            }
        }

        foreach (var property in type.GetProperties())
        {
            var defaultValueSql = property.PropertyInfo?.GetCustomAttribute<DefaultValueSqlAttribute>();
            if (defaultValueSql != null)
            {
                entity.Property(property.Name)
                    .HasDefaultValueSql(defaultValueSql.DefaultSql);
            }
        }

        return modelBuilder;
    }
}