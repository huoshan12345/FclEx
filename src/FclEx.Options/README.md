# FclEx.Options

Helpers for registering and configuring `Microsoft.Extensions.Options`.

## What Is Included

- `AddOptionsInstance` overloads for registering prebuilt or factory-created options instances.
- `InstanceOptionsFactory<TOptions>` for returning named options from a supplied factory.
- Service-aware `Configure<TOptions, TService>` registration.

## Notes

Use instance options when a component needs options that are already constructed outside the normal configuration binding path.
