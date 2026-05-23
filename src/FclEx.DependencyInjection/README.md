# FclEx.DependencyInjection

Convenience extensions for `Microsoft.Extensions.DependencyInjection`.

## What Is Included

- Short generic registration helpers for `Add`, `TryAdd`, and lifetime-specific registrations.
- Factory helpers such as `AddSingletonBy`, `AddScopedBy`, and `AddTransientBy`.
- Service replacement and removal helpers.
- Service wrapping helpers for decorating an existing registration.
- `IServiceProvider` and `IServiceScope` helper extensions.
- `ServiceDescriptorEqualityComparer` for comparing registrations.
- Source-generated overloads for factory helpers with multiple dependencies.

## Notes

The wrapping helpers are useful for lightweight decoration, but be mindful of service lifetimes when replacing or wrapping registrations.
