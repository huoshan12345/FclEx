namespace FclEx.Dapper;

/// <summary>
/// Represents explicitly applied FclEx Dapper type-map registrations.
/// </summary>
/// <remarks>
/// Equivalent concurrent registrations are reference counted. Disposing the last registration restores the
/// preceding type map unless another component replaced the FclEx map in the meantime. Disposal is idempotent.
/// </remarks>
public sealed class FclExDapperRegistration : IDisposable
{
    private IReadOnlyCollection<ColumnMappingRegistrationState>? _registrations;

    internal FclExDapperRegistration(IReadOnlyCollection<ColumnMappingRegistrationState> registrations)
    {
        _registrations = registrations;
    }

    /// <summary>
    /// Releases this registration and restores eligible preceding Dapper type maps.
    /// </summary>
    public void Dispose()
    {
        var registrations = Interlocked.Exchange(ref _registrations, null);
        if (registrations is not null)
            DapperHelper.ReleaseColumnMappings(registrations);
    }
}

internal sealed class ColumnMappingRegistrationState(
    EntityMapping mapping,
    SqlMapper.ITypeMap? previousMap,
    SqlMapper.ITypeMap appliedMap,
    ColumnMappingRegistrationState? previousRegistration)
{
    public EntityMapping Mapping { get; } = mapping;
    public SqlMapper.ITypeMap? PreviousMap { get; } = previousMap;
    public SqlMapper.ITypeMap AppliedMap { get; } = appliedMap;
    public ColumnMappingRegistrationState? PreviousRegistration { get; } = previousRegistration;
    public int ReferenceCount { get; set; } = 1;
}
