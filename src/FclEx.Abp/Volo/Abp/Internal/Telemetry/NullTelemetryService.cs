using System;
using System.Collections.Generic;

namespace Volo.Abp.Internal.Telemetry;

public class NullTelemetryService : ITelemetryService
{
    public IAsyncDisposable TrackActivityAsync(string activityName, Action<Dictionary<string, object>>? additionalProperties = null)
    {
        return AsyncDisposable.Empty;
    }

    public Task AddActivityAsync(string activityName, Action<Dictionary<string, object>>? additionalProperties = null)
    {
        return Task.CompletedTask;
    }

    public Task AddErrorActivityAsync(Action<Dictionary<string, object>> additionalProperties)
    {
        return Task.CompletedTask;
    }

    public Task AddErrorActivityAsync(string errorMessage)
    {
        return Task.CompletedTask;
    }

    public Task AddErrorForActivityAsync(string failingActivity, string errorMessage)
    {
        return Task.CompletedTask;
    }
}
