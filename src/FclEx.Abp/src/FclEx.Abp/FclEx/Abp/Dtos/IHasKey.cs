namespace FclEx.Abp.Dtos;

public interface IHasKey<TPrimaryKey>
{
    TPrimaryKey Id { get; set; }
}