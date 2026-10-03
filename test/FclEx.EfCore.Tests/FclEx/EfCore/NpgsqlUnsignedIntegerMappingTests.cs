namespace FclEx.EfCore;

public class NpgsqlUnsignedIntegerMappingTests
{
    [Theory]
    [InlineData(nameof(UnsignedEntity.ByteValue), "smallint", typeof(byte), byte.MaxValue)]
    [InlineData(nameof(UnsignedEntity.UInt16Value), "integer", typeof(int), ushort.MaxValue)]
    [InlineData(nameof(UnsignedEntity.UInt32Value), "bigint", typeof(long), uint.MaxValue)]
    [InlineData(nameof(UnsignedEntity.UInt64Value), "numeric(20,0)", typeof(decimal), ulong.MaxValue)]
    public void DefaultMapping_UsesLosslessProviderType(
        string propertyName, string storeType, Type providerType, object value)
    {
        // Model construction does not open a database connection.
        var options = new DbContextOptionsBuilder<UnsignedMappingContext>()
            .UseNpgsql("Host=localhost;Database=unused")
            .Options;
        using var context = new UnsignedMappingContext(options);
        var property = context.Model.FindEntityType(typeof(UnsignedEntity))!.FindProperty(propertyName)!;
        var mapping = property.GetRelationalTypeMapping();

        Assert.Equal(storeType, mapping.StoreType);
        var convertedValue = mapping.Converter is { } converter ? converter.ConvertToProvider(value) : value;
        Assert.IsType(providerType, convertedValue);
        Assert.Equal(Convert.ToDecimal(value), Convert.ToDecimal(convertedValue));
    }

    private sealed class UnsignedMappingContext(DbContextOptions<UnsignedMappingContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UnsignedEntity>();
        }
    }

    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Local")]
    private sealed class UnsignedEntity
    {
        public int Id { get; set; }
        public byte ByteValue { get; set; }
        public ushort UInt16Value { get; set; }
        public uint UInt32Value { get; set; }
        public ulong UInt64Value { get; set; }
    }
}
