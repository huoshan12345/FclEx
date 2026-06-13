# FclEx.Abp

ABP integration helpers for FclEx applications.

## What Is Included

- `FclExAbpModule` for wiring FclEx conventions into ABP modules.
- Conventional registration helpers for open generic and generic-interface services.
- AspectCore-based interceptor attributes for login-and-retry flows and return-value caching.
- ABP service collection, service provider, configuration, directory, and initialization-context extensions.
- Null telemetry service implementations for suppressing ABP telemetry activity.
- Build-transitive props and targets that provide ABP-oriented generated usings.

## Usage Notes

- This package depends on `FclEx.Caching`, `FclEx.Http`, ABP Core, and AspectCore.
- Use it in ABP applications where FclEx conventions and interceptors should participate in module startup.
- The AOP attributes require AspectCore integration to be active in the host service provider.
