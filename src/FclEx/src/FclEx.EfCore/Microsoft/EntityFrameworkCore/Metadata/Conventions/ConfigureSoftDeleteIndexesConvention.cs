namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
/// A convention that configures soft delete indexes during the model finalizing stage.
/// This class ensures that indexes related to soft deletion (e.g., <see cref="ISoftDeletable.IsDeleted"/> 
/// and <see cref="IHasDeletedAt.DeletedAt"/>) are correctly handled for entity types that implement 
/// these interfaces.
/// 
/// <para>
/// Note: Removing existing indexes in the <c>OnModelCreating</c> method does not work as expected. 
/// Therefore, this convention is used during the model finalizing phase to address this limitation.
/// </para>
/// </summary>
public class ConfigureSoftDeleteIndexesConvention : IModelFinalizingConvention
{
    public static readonly ConfigureSoftDeleteIndexesConvention Instance = new();

    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var type in modelBuilder.Metadata.GetEntityTypes())
        {
            modelBuilder.ConfigureSoftDeleteIndexes(type);
        }
    }
}

