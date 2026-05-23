# FclEx.Options

Helpers for registering and configuring `Microsoft.Extensions.Options`.

## What Is Included

- `AddOptionsInstance` overloads for registering prebuilt or factory-created options instances.
- `InstanceOptionsFactory<TOptions>` for returning named options from a supplied factory.
- Service-aware `Configure<TOptions, TService>` registration.

## Target Frameworks

By default this project targets `netstandard2.0`, `net472`, `net8.0`, `net9.0`, and `net10.0`.

## Dependencies

- `Microsoft.Extensions.Options`
- `FclEx.DependencyInjection`

## Notes

Use instance options when a component needs options that are already constructed outside the normal configuration binding path.
