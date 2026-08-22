namespace FclEx.EfCore;

public static class EntityTypeExtensions
{
    public static void ApplyKeyTo<T>(this IEntityType entityType, T source, T target, Func<IProperty, object?, object?>? transform = null) where T : class
    {
        if (entityType.ClrType != typeof(T))
            throw new InvalidOperationException($"EntityType CLR type {entityType.ClrType.Name} does not match {typeof(T).Name}");

        var key = entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException($"{typeof(T).Name} does not have a primary key defined");

        transform ??= (_, value) => value;
        foreach (var property in key.Properties)
        {
            if (property.IsShadowProperty())
                throw new InvalidOperationException($"Cannot apply key: '{property.Name}' on {typeof(T).Name} is a shadow property " +
                                                    $"and has no CLR backing member to read from or write to on a plain object instance.");

            var value = property.GetValue(source);
            var newValue = transform(property, value);
            property.SetValue(target, newValue);
        }
    }
}
