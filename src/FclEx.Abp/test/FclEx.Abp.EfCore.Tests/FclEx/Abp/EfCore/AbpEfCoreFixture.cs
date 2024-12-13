namespace FclEx.Abp.EfCore;

public class AbpEfCoreFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await using var context = new GlobalDbContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}