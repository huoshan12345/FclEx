namespace FclEx.EfCore.Extensions.DbContextExtensions;

partial class ApplyChangesSqliteTests
{
    private sealed record ArticleTagDto(string TagId, string Note);

    private sealed record GraphBranchDto(string Id, string Name);

    private sealed record GraphLeafMoveDto(string Id, string BranchId, string Name);

    private sealed class Article
    {
        public string Id { get; set; } = "";

        public string Name { get; set; } = "";

        public List<Tag> Tags { get; set; } = [];

        public List<ArticleTag> ArticleTags { get; set; } = [];
    }

    private sealed class Tag
    {
        public string Id { get; set; } = "";

        public string Name { get; set; } = "";

        public List<Article> Articles { get; set; } = [];

        public List<ArticleTag> ArticleTags { get; set; } = [];
    }

    private sealed class ArticleTag
    {
        public string ArticleId { get; set; } = "";

        public string TagId { get; set; } = "";

        public string Note { get; set; } = "";

        public Article Article { get; set; } = null!;

        public Tag Tag { get; set; } = null!;
    }

    private sealed class GraphRoot
    {
        public string Id { get; set; } = "";

        public string Name { get; set; } = "";

        public List<GraphBranch> Branches { get; set; } = [];
    }

    private sealed class GraphBranch
    {
        public string Id { get; set; } = "";

        public string RootId { get; set; } = "";

        public string Name { get; set; } = "";

        public GraphRoot Root { get; set; } = null!;

        public List<GraphLeaf> Leaves { get; set; } = [];
    }

    private sealed class GraphLeaf
    {
        public string Id { get; set; } = "";

        public string BranchId { get; set; } = "";

        public string Name { get; set; } = "";

        public GraphBranch Branch { get; set; } = null!;
    }

    private static void ConfigureRelationshipModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(builder =>
        {
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.Name).IsRequired();
            builder.HasMany(entity => entity.Tags)
                .WithMany(entity => entity.Articles)
                .UsingEntity<ArticleTag>(
                    right => right.HasOne(entity => entity.Tag)
                        .WithMany(entity => entity.ArticleTags)
                        .HasForeignKey(entity => entity.TagId)
                        .OnDelete(DeleteBehavior.Cascade),
                    left => left.HasOne(entity => entity.Article)
                        .WithMany(entity => entity.ArticleTags)
                        .HasForeignKey(entity => entity.ArticleId)
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.HasKey(entity => new { entity.ArticleId, entity.TagId });
                        join.Property(entity => entity.Note).IsRequired();
                    });
        });

        modelBuilder.Entity<Tag>(builder =>
        {
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.Name).IsRequired();
        });

        modelBuilder.Entity<GraphRoot>(builder =>
        {
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.Name).IsRequired();
        });

        modelBuilder.Entity<GraphBranch>(builder =>
        {
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.Name).IsRequired();
            builder.HasOne(entity => entity.Root)
                .WithMany(entity => entity.Branches)
                .HasForeignKey(entity => entity.RootId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GraphLeaf>(builder =>
        {
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever();
            builder.Property(entity => entity.Name).IsRequired();
            builder.HasOne(entity => entity.Branch)
                .WithMany(entity => entity.Leaves)
                .HasForeignKey(entity => entity.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
