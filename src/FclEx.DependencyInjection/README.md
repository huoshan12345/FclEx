# FclEx.DependencyInjection

Dependency-injection conveniences for `Microsoft.Extensions.DependencyInjection`.

## What Is Included

- Service registration helpers for common lifetime patterns.
- Service replacement and removal helpers.
- Decoration helpers.
- Scope and service-provider lookup helpers.
- Source-generated overloads for service registration APIs.

## Usage Notes

- This package is a small layer over the built-in Microsoft DI abstractions.
- It does not replace the container or require a separate service-provider implementation.
- `FclEx.Options`, `FclEx.Logging`, and other packages build on these helpers.
