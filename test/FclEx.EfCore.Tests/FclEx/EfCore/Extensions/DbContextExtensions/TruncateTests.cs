using static FclEx.EfCore.Extensions.TruncateTestContext;

namespace FclEx.EfCore.Extensions;

public partial class DbContextTruncateTests
{
    public static TheoryData<int, string> ProviderCases => new()
    {
        { 0, "TRUNCATE TABLE [Tenant].[Items];" },
        { 1, "TRUNCATE TABLE \"Tenant\".\"Items\";" },
        { 2, "TRUNCATE TABLE `Items`;" },
        { 3, "TRUNCATE TABLE `Items`;" },
    };

    public static TheoryData<int, bool, bool, string?> OptionCases
    {
        get
        {
            var cases = new TheoryData<int, bool, bool, string?>
            {
                { 1, false, false, "TRUNCATE TABLE \"Tenant\".\"Items\" CONTINUE IDENTITY;" },
                { 1, false, true, "TRUNCATE TABLE \"Tenant\".\"Items\" CONTINUE IDENTITY CASCADE;" },
                { 1, true, false, "TRUNCATE TABLE \"Tenant\".\"Items\" RESTART IDENTITY;" },
                { 1, true, true, "TRUNCATE TABLE \"Tenant\".\"Items\" RESTART IDENTITY CASCADE;" },
            };
            foreach (var provider in new[] { 0, 2, 3, 4, 5 })
            {
                foreach (var restart in new[] { false, true })
                {
                    foreach (var cascade in new[] { false, true })
                    {
                        var expected = (provider, restart, cascade) switch
                        {
                            (0, true, false) => "TRUNCATE TABLE [Tenant].[Items];",
                            (2 or 3, true, false) => "TRUNCATE TABLE `Items`;",
                            _ => null,
                        };
                        cases.Add(provider, restart, cascade, expected);
                    }
                }
            }
            return cases;
        }
    }

    [Theory]
    [MemberData(nameof(OptionCases))]
    public async Task TruncateAsync_ExplicitOptionsHonorProviderCapabilities(
        int provider, bool restartIdentity, bool cascade, string? expected)
    {
        var recorder = new CommandRecorder();
        await using var context = Create(provider, "simple", recorder);
        var item = new Item { Id = 1 };
        context.Attach(item);
        using var source = new CancellationTokenSource();

        if (expected is null)
        {
            await Assert.ThrowsAsync<NotSupportedException>(() =>
                context.TruncateAsync<Item>(restartIdentity, cascade, source.Token));
            await Assert.ThrowsAsync<NotSupportedException>(() =>
                context.TruncateAsync(typeof(Item), restartIdentity, cascade, source.Token));
            Assert.Empty(recorder.Commands);
            Assert.Equal(0, recorder.OpenCount);
        }
        else
        {
            await context.TruncateAsync<Item>(restartIdentity, cascade, source.Token);
            await context.TruncateAsync(typeof(Item), restartIdentity, cascade, source.Token);
            Assert.Equal([expected, expected], recorder.Commands);
            Assert.Equal(source.Token, recorder.CommandToken);
        }
        Assert.Equal(EntityState.Unchanged, context.Entry(item).State);
    }

    [Theory]
    [MemberData(nameof(ProviderCases))]
    public async Task TruncateAsync_UsesNativeSqlAndPreservesTrackedEntities(int provider, string expected)
    {
        var recorder = new CommandRecorder();
        await using var context = Create(provider, "simple", recorder);
        var item = new Item { Id = 1 };
        context.Attach(item);
        using var source = new CancellationTokenSource();

        await context.TruncateAsync<Item>(source.Token);

        Assert.Equal([expected], recorder.Commands);
        Assert.Equal(source.Token, recorder.CommandToken);
        Assert.Equal(EntityState.Unchanged, context.Entry(item).State);
    }

    [Theory]
    [InlineData(0, "TRUNCATE TABLE [Tenant].[Items]]\"`];")]
    [InlineData(1, "TRUNCATE TABLE \"Tenant\".\"Items]\"\"`\";")]
    [InlineData(2, "TRUNCATE TABLE `Items]\"```;")]
    [InlineData(3, "TRUNCATE TABLE `Items]\"```;")]
    public async Task TruncateAsync_TypeOverloadEscapesIdentifiers(int provider, string expected)
    {
        var recorder = new CommandRecorder();
        await using var context = Create(provider, "escaped", recorder);

        await context.TruncateAsync(typeof(Item));

        Assert.Equal([expected], recorder.Commands);
    }

    [Theory]
    [InlineData("tph")]
    [InlineData("tpt")]
    [InlineData("tpc")]
    [InlineData("split")]
    [InlineData("owned")]
    [InlineData("table-sharing")]
    [InlineData("view")]
    public async Task TruncateAsync_RejectsUnsupportedMappingsBeforeOpeningConnection(string shape)
    {
        var recorder = new CommandRecorder();
        await using var context = Create(0, shape, recorder);

        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync<Item>());
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync<Item>(true, false));
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync(typeof(Item), true, false));
        if (shape is "tph" or "tpt" or "tpc")
            await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync<DerivedItem>());

        Assert.Empty(recorder.Commands);
        Assert.Equal(0, recorder.OpenCount);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    public async Task TruncateAsync_RejectsUnsupportedProviderWithoutDeletingRows(int provider)
    {
        var recorder = new CommandRecorder();
        await using var context = Create(provider, "simple", recorder);

        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync<Item>());

        Assert.Empty(recorder.Commands);
        Assert.Equal(0, recorder.OpenCount);
    }

    [Fact]
    public async Task TruncateAsync_RejectsUnknownEntityAndAmbiguousSharedClrType()
    {
        var recorder = new CommandRecorder();
        await using var context = Create(0, "shared-type", recorder);

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.TruncateAsync<Item>());
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.TruncateAsync<Dictionary<string, object>>());
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.TruncateAsync<Item>(true, false));
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.TruncateAsync(typeof(Dictionary<string, object>), true, false));

        Assert.Equal(0, recorder.OpenCount);
    }

    [Fact]
    public async Task TruncateAsync_ValidatesNullArguments()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync<Item>());
        var recorder = new CommandRecorder();
        await using var context = Create(0, "simple", recorder);
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.TruncateAsync((Type)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync<Item>(true, false));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync(typeof(Item), true, false));
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.TruncateAsync((Type)null!, true, false));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task TruncateAsync_ObservesCancellation(int provider)
    {
        var recorder = new CommandRecorder();
        await using var context = Create(provider, "simple", recorder);
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.TruncateAsync<Item>(source.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.TruncateAsync<Item>(true, false, source.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.TruncateAsync(typeof(Item), true, false, source.Token));

        Assert.Empty(recorder.Commands);
    }
}
