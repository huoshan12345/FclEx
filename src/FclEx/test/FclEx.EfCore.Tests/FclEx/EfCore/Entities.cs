namespace FclEx.EfCore;

public class EntityWithAutoKey
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Value { get; set; }
}