using static FclEx.EfCore.Extensions.TruncateTestContext;

namespace FclEx.EfCore.Extensions;

public class DbSetExtensionsTests
{
    [Theory]
    [MemberData(nameof(DbContextTruncateTests.OptionCases), MemberType = typeof(DbContextTruncateTests))]
    public async Task TruncateAsync_ExplicitOptionsHonorProviderCapabilities(
        int provider, bool restartIdentity, bool cascade, string? expected)
    {
        var recorder = new CommandRecorder();
        await using var context = Create(provider, "simple", recorder);
        using var source = new CancellationTokenSource();

        if (expected is null)
        {
            await Assert.ThrowsAsync<NotSupportedException>(() =>
                context.Set<Item>().TruncateAsync(restartIdentity, cascade, source.Token));
            Assert.Empty(recorder.Commands);
            Assert.Equal(0, recorder.OpenCount);
        }
        else
        {
            await context.Set<Item>().TruncateAsync(restartIdentity, cascade, source.Token);
            Assert.Equal([expected], recorder.Commands);
            Assert.Equal(source.Token, recorder.CommandToken);
        }
    }

    [Theory]
    [MemberData(nameof(DbContextTruncateTests.ProviderCases), MemberType = typeof(DbContextTruncateTests))]
    public async Task TruncateAsync_UsesSetMetadataAndCancellation(int provider, string expected)
    {
        var recorder = new CommandRecorder();
        await using var context = Create(provider, "simple", recorder);
        using var source = new CancellationTokenSource();

        await context.Set<Item>().TruncateAsync(source.Token);

        Assert.Equal([expected], recorder.Commands);
        Assert.Equal(source.Token, recorder.CommandToken);
    }

    [Fact]
    public async Task TruncateAsync_PreservesNamedSharedTypeEntityIdentity()
    {
        var recorder = new CommandRecorder();
        await using var context = Create(0, "shared-type", recorder);

        await context.Set<Dictionary<string, object>>("Second").TruncateAsync();
        await context.Set<Dictionary<string, object>>("Second").TruncateAsync(true, false);

        Assert.Equal(["TRUNCATE TABLE [Tenant].[Second];", "TRUNCATE TABLE [Tenant].[Second];"], recorder.Commands);
    }

    [Fact]
    public async Task TruncateAsync_RejectsSharedPhysicalTable()
    {
        var recorder = new CommandRecorder();
        await using var context = Create(0, "owned", recorder);

        await Assert.ThrowsAsync<NotSupportedException>(() => context.Set<Item>().TruncateAsync());
        await Assert.ThrowsAsync<NotSupportedException>(() => context.Set<Item>().TruncateAsync(true, false));

        Assert.Equal(0, recorder.OpenCount);
    }

    [Fact]
    public async Task TruncateAsync_ValidatesNullSet()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbSet<Item>)null!).TruncateAsync());
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbSet<Item>)null!).TruncateAsync(true, false));
    }

    [Fact]
    public async Task TruncateAsync_RejectsSqlite()
    {
        var recorder = new CommandRecorder();
        await using var context = Create(4, "simple", recorder);

        await Assert.ThrowsAsync<NotSupportedException>(() => context.Set<Item>().TruncateAsync());

        Assert.Equal(0, recorder.OpenCount);
    }

    [Fact]
    public async Task TruncateAsync_ObservesCancellation()
    {
        var recorder = new CommandRecorder();
        await using var context = Create(0, "simple", recorder);
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.Set<Item>().TruncateAsync(source.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.Set<Item>().TruncateAsync(true, false, source.Token));

        Assert.Empty(recorder.Commands);
    }
}
