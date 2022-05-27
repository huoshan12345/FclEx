using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite
{
    public enum Status
    {
        None, 
        Working,
        Studying,
        Sleeping
    }

    public class TestEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = nameof(TestEntity);

        public int Age { get; set; }
        public string Gender { get; set; } = "unknown";
        public DateTime CreationTime { get; set; }

        public int? NullableInt { get; set; }
        public DateTime? NullableDateTime { get; set; }
    }

    public class TestEntityWithGuidKey
    {
        [PrimaryKey, AutoId]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = nameof(TestEntity);

        public int Age { get; set; }
        public string Gender { get; set; } = "unknown";
        public DateTime CreationTime { get; set; }

        public int? NullableInt { get; set; }
        public DateTime? NullableDateTime { get; set; }
    }
}
