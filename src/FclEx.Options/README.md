# FclEx.Options

Options registration helpers for `Microsoft.Extensions.Options`.

## What Is Included

- Register prebuilt options instances.
- Register options created by factories.
- Configure options from services.
- Resolve option values from service providers.
- `InstanceOptionsFactory<TOptions>` for options backed by an existing instance.

## Usage Notes

- This package builds on `Microsoft.Extensions.Options` and `FclEx.DependencyInjection`.
- Use it when options need to be composed from existing services or prebuilt objects.
- Prefer the built-in options APIs for simple configuration binding.
