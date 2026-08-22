namespace FclEx.EfCore.Extensions.DbContextExtensions;

public partial class ApplyChangesSqliteTests
{
    public enum RelationshipTrackingMode
    {
        All,
        None,
        UpdateOnly,
        DeleteOnly,
        DetachedCopiesWithTrackedCounterparts,
    }

    public static TheoryData<bool, RelationshipTrackingMode, bool> ManyToManyTrackingCases
    {
        get
        {
            var data = new TheoryData<bool, RelationshipTrackingMode, bool>();
            foreach (var principalsTracked in new[] { false, true })
            {
                foreach (var trackingMode in Enum.GetValues<RelationshipTrackingMode>())
                {
                    data.Add(principalsTracked, trackingMode, false);
                    data.Add(principalsTracked, trackingMode, true);
                }
            }
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(ManyToManyTrackingCases))]
    public async Task ApplyChanges_ShouldSynchronizeManyToManyJoinEntitiesAcrossTrackingCombinations(
        bool principalsTracked,
        RelationshipTrackingMode trackingMode,
        bool useEntityOverload)
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await SeedManyToManyAsync(database);

        await using (var context = database.CreateContext())
        {
            if (principalsTracked)
            {
                _ = await context.Articles.SingleAsync(entity => entity.Id == "article");
                _ = await context.Tags.OrderBy(entity => entity.Id).ToListAsync();
            }
            else
            {
                _ = await context.Articles.AsNoTracking().SingleAsync(entity => entity.Id == "article");
                _ = await context.Tags.AsNoTracking().OrderBy(entity => entity.Id).ToListAsync();
            }

            var existingLinks = await LoadArticleTagsAsync(context, trackingMode);
            Assert.Equal(principalsTracked ? 1 : 0, context.ChangeTracker.Entries<Article>().Count());
            Assert.Equal(principalsTracked ? 3 : 0, context.ChangeTracker.Entries<Tag>().Count());
            Assert.Equal(ExpectedTrackedRelationshipCount(trackingMode), context.ChangeTracker.Entries<ArticleTag>().Count());

            EntityChanges<ArticleTag> changes;
            if (useEntityOverload)
            {
                changes = context.ApplyChanges(
                    [
                        new ArticleTag { ArticleId = "article", TagId = "tag-update", Note = "After" },
                        new ArticleTag { ArticleId = "article", TagId = "tag-insert", Note = "Insert" },
                    ],
                    existingLinks,
                    entity => entity.TagId,
                    allowDeletion: true);
            }
            else
            {
                changes = context.ApplyChanges(
                    [new ArticleTagDto("tag-update", "After"), new ArticleTagDto("tag-insert", "Insert")],
                    dto => dto.TagId,
                    existingLinks,
                    entity => entity.TagId,
                    dto => new ArticleTag { ArticleId = "article", TagId = dto.TagId, Note = dto.Note },
                    (dto, entity) =>
                    {
                        entity.Note = dto.Note;
                        return entity;
                    },
                    allowDeletion: true);
            }

            var inserted = Assert.Single(changes.Inserted);
            var updated = Assert.Single(changes.Updated).New;
            var deleted = Assert.Single(changes.Deleted);
            Assert.Equal("tag-insert", inserted.TagId);
            Assert.Equal("tag-update", updated.TagId);
            Assert.Equal("tag-delete", deleted.TagId);
            Assert.Equal(EntityState.Added, context.Entry(inserted).State);
            Assert.Equal(EntityState.Modified, context.Entry(updated).State);
            Assert.Equal(EntityState.Deleted, context.Entry(deleted).State);
            Assert.All(context.ChangeTracker.Entries<Article>(), entry => Assert.Equal(EntityState.Unchanged, entry.State));
            Assert.All(context.ChangeTracker.Entries<Tag>(), entry => Assert.Equal(EntityState.Unchanged, entry.State));

            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        var links = await verificationContext.ArticleTags
            .AsNoTracking()
            .Where(entity => entity.ArticleId == "article")
            .OrderBy(entity => entity.TagId)
            .Select(entity => new { entity.TagId, entity.Note })
            .ToListAsync();
        var relatedTagIds = await verificationContext.Articles
            .Where(entity => entity.Id == "article")
            .SelectMany(entity => entity.Tags)
            .OrderBy(entity => entity.Id)
            .Select(entity => entity.Id)
            .ToListAsync();

        Assert.Equal(2, links.Count);
        Assert.Equal("Insert", links.Single(entity => entity.TagId == "tag-insert").Note);
        Assert.Equal("After", links.Single(entity => entity.TagId == "tag-update").Note);
        Assert.Equal(["tag-insert", "tag-update"], relatedTagIds);
        Assert.Equal(3, await verificationContext.Tags.CountAsync());
    }

    [Fact]
    public async Task DtoOverload_ShouldDeleteOnlyManyToManyJoinWhenTargetIsShared()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await SeedManyToManyAsync(database);

        await using (var context = database.CreateContext())
        {
            context.Add(new Article { Id = "other-article", Name = "Other" });
            context.Add(new ArticleTag
            {
                ArticleId = "other-article",
                TagId = "tag-delete",
                Note = "Shared",
            });
            await context.SaveChangesAsync();
        }

        await using (var context = database.CreateContext())
        {
            var existing = await context.ArticleTags
                .AsNoTracking()
                .Where(entity => entity.ArticleId == "article" && entity.TagId == "tag-delete")
                .ToListAsync();

            var changes = context.ApplyChanges(
                Array.Empty<ArticleTagDto>(),
                dto => dto.TagId,
                existing,
                entity => entity.TagId,
                dto => new ArticleTag { ArticleId = "article", TagId = dto.TagId, Note = dto.Note },
                allowDeletion: true);

            Assert.Equal(EntityState.Deleted, context.Entry(Assert.Single(changes.Deleted)).State);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        Assert.True(await verificationContext.Tags.AnyAsync(entity => entity.Id == "tag-delete"));
        Assert.True(await verificationContext.ArticleTags.AnyAsync(entity =>
            entity.ArticleId == "other-article" && entity.TagId == "tag-delete"));
        Assert.False(await verificationContext.ArticleTags.AnyAsync(entity =>
            entity.ArticleId == "article" && entity.TagId == "tag-delete"));
    }

    private static async Task SeedManyToManyAsync(SqliteTestDatabase database)
    {
        await using var context = database.CreateContext();
        context.Add(new Article { Id = "article", Name = "Article" });
        context.AddRange(
            new Tag { Id = "tag-update", Name = "Update" },
            new Tag { Id = "tag-delete", Name = "Delete" },
            new Tag { Id = "tag-insert", Name = "Insert" });
        context.AddRange(
            new ArticleTag { ArticleId = "article", TagId = "tag-update", Note = "Before" },
            new ArticleTag { ArticleId = "article", TagId = "tag-delete", Note = "Delete" });
        await context.SaveChangesAsync();
    }

    private static async Task<List<ArticleTag>> LoadArticleTagsAsync(
        ApplyChangesDbContext context,
        RelationshipTrackingMode trackingMode)
    {
        IQueryable<ArticleTag> Links() => context.ArticleTags
            .Where(entity => entity.ArticleId == "article")
            .OrderBy(entity => entity.TagId);

        switch (trackingMode)
        {
            case RelationshipTrackingMode.All:
                return await Links().ToListAsync();
            case RelationshipTrackingMode.None:
                return await Links().AsNoTracking().ToListAsync();
            case RelationshipTrackingMode.UpdateOnly:
            {
                var update = await Links().SingleAsync(entity => entity.TagId == "tag-update");
                var delete = await Links().AsNoTracking().SingleAsync(entity => entity.TagId == "tag-delete");
                return [update, delete];
            }
            case RelationshipTrackingMode.DeleteOnly:
            {
                var update = await Links().AsNoTracking().SingleAsync(entity => entity.TagId == "tag-update");
                var delete = await Links().SingleAsync(entity => entity.TagId == "tag-delete");
                return [update, delete];
            }
            case RelationshipTrackingMode.DetachedCopiesWithTrackedCounterparts:
                _ = await Links().ToListAsync();
                return await Links().AsNoTracking().ToListAsync();
            default:
                throw new ArgumentOutOfRangeException(nameof(trackingMode), trackingMode, null);
        }
    }

    private static int ExpectedTrackedRelationshipCount(RelationshipTrackingMode trackingMode)
    {
        return trackingMode switch
        {
            RelationshipTrackingMode.All or RelationshipTrackingMode.DetachedCopiesWithTrackedCounterparts => 2,
            RelationshipTrackingMode.None => 0,
            RelationshipTrackingMode.UpdateOnly or RelationshipTrackingMode.DeleteOnly => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(trackingMode), trackingMode, null),
        };
    }
}
