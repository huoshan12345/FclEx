namespace FclEx.Abp.Domain;

public static class EntityMemberNames
{
    public const string Id = nameof(IEntity<int>.Id);
    public const string CreationTime = nameof(IHasCreationTime.CreationTime);
    public const string DeletionTime = nameof(IHasDeletionTime.DeletionTime);
    public const string LastModificationTime = nameof(IHasModificationTime.LastModificationTime);
    public const string LastModifierUserId = nameof(IModificationAudited<int>.LastModifierUserId);
    public const string IsActive = nameof(IPassivable.IsActive);
    public const string IsDeleted = nameof(ISoftDelete.IsDeleted);
}