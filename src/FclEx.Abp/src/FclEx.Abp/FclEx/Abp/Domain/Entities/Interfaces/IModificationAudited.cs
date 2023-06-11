namespace FclEx.Abp.Domain.Entities.Interfaces;

public interface IModificationAudited<TUserId> : IHasModificationTime
{
    TUserId LastModifierUserId { get; set; }
}