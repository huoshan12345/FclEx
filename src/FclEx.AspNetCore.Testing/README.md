# FclEx.AspNetCore.Testing

Integration-test helpers for ASP.NET Core applications.

## What Is Included

- `TestWebApplicationFactory<TStartup>` for `WebApplicationFactory`-based tests.
- Test content-root and web-host builder helpers.
- `AllowExternalHandler` for forwarding selected HTTP requests outside the in-memory test server.
- Integration with the ASP.NET Core and HTTP helpers from FclEx.

## Usage Notes

- This package targets `net8.0`, `net9.0`, and `net10.0`.
- It is intended for test projects rather than production deployments.
- Use `AllowExternalHandler` only when tests intentionally call external services.
