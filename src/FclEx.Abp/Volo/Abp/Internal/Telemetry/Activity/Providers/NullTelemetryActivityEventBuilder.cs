namespace Volo.Abp.Internal.Telemetry.Activity.Providers;

public class NullTelemetryActivityEventBuilder : ITelemetryActivityEventBuilder
{
    public Task<ActivityEvent?> BuildAsync(ActivityContext context)
    {
        return Task.FromResult(default(ActivityEvent?));
    }
}
