using Microsoft.EntityFrameworkCore.Metadata;
using static FclEx.EfCore.Extensions.TruncateTestContext;

namespace FclEx.EfCore.Extensions;

public partial class DbContextTruncateTests
{
    [Theory]
    [MemberData(nameof(ProviderCases))]
    public async Task TruncateAsync_MetadataOverloadUsesNativeSql(int provider, string expected)
    {
        var recorder = new CommandRecorder();
        await using var context = Create(provider, "simple", recorder);
        var entityType = context.Model.FindEntityType(typeof(Item))!;
        var item = new Item { Id = 1 };
        context.Attach(item);
        using var source = new CancellationTokenSource();

        await context.TruncateAsync(entityType, source.Token);

        Assert.Equal([expected], recorder.Commands);
        Assert.Equal(source.Token, recorder.CommandToken);
        Assert.Equal(EntityState.Unchanged, context.Entry(item).State);
    }

    [Theory]
    [MemberData(nameof(OptionCases))]
    public async Task TruncateAsync_MetadataOptionsHonorProviderCapabilities(
        int provider, bool restartIdentity, bool cascade, string? expected)
    {
        var recorder = new CommandRecorder();
        await using var context = Create(provider, "simple", recorder);
        var entityType = context.Model.FindEntityType(typeof(Item))!;
        using var source = new CancellationTokenSource();

        if (expected is null)
        {
            await Assert.ThrowsAsync<NotSupportedException>(() =>
                context.TruncateAsync(entityType, restartIdentity, cascade, source.Token));
            Assert.Empty(recorder.Commands);
            Assert.Equal(0, recorder.OpenCount);
        }
        else
        {
            await context.TruncateAsync(entityType, restartIdentity, cascade, source.Token);
            Assert.Equal([expected], recorder.Commands);
            Assert.Equal(source.Token, recorder.CommandToken);
        }
    }

    [Fact]
    public async Task TruncateAsync_MetadataOverloadsPreserveNamedSharedTypeIdentity()
    {
        var recorder = new CommandRecorder();
        await using var context = Create(1, "shared-type", recorder);
        var entityType = context.Model.FindEntityType("Second")!;

        await context.TruncateAsync(entityType);
        await context.TruncateAsync(entityType, true, true);

        Assert.Equal([
            "TRUNCATE TABLE \"Tenant\".\"Second\";",
            "TRUNCATE TABLE \"Tenant\".\"Second\" RESTART IDENTITY CASCADE;",
        ], recorder.Commands);
    }

    [Theory]
    [InlineData("tph")]
    [InlineData("tpt")]
    [InlineData("tpc")]
    [InlineData("split")]
    [InlineData("owned")]
    [InlineData("table-sharing")]
    [InlineData("view")]
    public async Task TruncateAsync_MetadataOverloadsRejectUnsupportedMappings(string shape)
    {
        var recorder = new CommandRecorder();
        await using var context = Create(0, shape, recorder);
        var entityType = context.Model.FindEntityType(typeof(Item))!;

        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync(entityType));
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync(entityType, true, false));

        Assert.Empty(recorder.Commands);
        Assert.Equal(0, recorder.OpenCount);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    public async Task TruncateAsync_MetadataNativeOverloadRejectsUnsupportedProviders(int provider)
    {
        var recorder = new CommandRecorder();
        await using var context = Create(provider, "simple", recorder);
        var entityType = context.Model.FindEntityType(typeof(Item))!;

        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync(entityType));

        Assert.Equal(0, recorder.OpenCount);
    }

    [Fact]
    public async Task TruncateAsync_MetadataOverloadsValidateNullArguments()
    {
        var recorder = new CommandRecorder();
        await using var context = Create(0, "simple", recorder);
        var entityType = context.Model.FindEntityType(typeof(Item))!;

        await Assert.ThrowsAsync<ArgumentNullException>(() => context.TruncateAsync((IEntityType)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.TruncateAsync((IEntityType)null!, true, false));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync(entityType));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync(entityType, true, false));

        Assert.Equal(0, recorder.OpenCount);
    }

    [Fact]
    public async Task TruncateAsync_MetadataOverloadsRejectMetadataFromAnotherModel()
    {
        var recorder = new CommandRecorder();
        await using var context = Create(0, "simple", recorder);
        await using var other = Create(0, "escaped", new CommandRecorder());
        var entityType = other.Model.FindEntityType(typeof(Item))!;

        var nativeException = await Assert.ThrowsAsync<ArgumentException>(() => context.TruncateAsync(entityType));
        var optionsException = await Assert.ThrowsAsync<ArgumentException>(() => context.TruncateAsync(entityType, true, false));

        Assert.Equal("entityType", nativeException.ParamName);
        Assert.Equal("entityType", optionsException.ParamName);
        Assert.Empty(recorder.Commands);
        Assert.Equal(0, recorder.OpenCount);
    }

    [Fact]
    public async Task TruncateAsync_MetadataOverloadsAcceptMetadataFromSharedModel()
    {
        var recorder = new CommandRecorder();
        await using var context = Create(0, "simple", recorder);
        await using var other = Create(0, "simple", new CommandRecorder());
        Assert.Same(context.Model, other.Model);
        var entityType = other.Model.FindEntityType(typeof(Item))!;

        await context.TruncateAsync(entityType);
        await context.TruncateAsync(entityType, true, false);

        Assert.Equal(["TRUNCATE TABLE [Tenant].[Items];", "TRUNCATE TABLE [Tenant].[Items];"], recorder.Commands);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task TruncateAsync_MetadataOverloadsObserveCancellation(int provider)
    {
        var recorder = new CommandRecorder();
        await using var context = Create(provider, "simple", recorder);
        var entityType = context.Model.FindEntityType(typeof(Item))!;
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.TruncateAsync(entityType, source.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.TruncateAsync(entityType, true, false, source.Token));

        Assert.Empty(recorder.Commands);
    }
}
