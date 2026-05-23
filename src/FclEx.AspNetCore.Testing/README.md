# FclEx.AspNetCore.Testing

Testing helpers for ASP.NET Core applications that use `WebApplicationFactory`.

## What Is Included

- `TestWebApplicationFactory<TStartup>` for configuring application factories in integration tests.
- Content-root helpers for test hosts.
- `AllowExternalHandler`, a delegating handler that can pass selected requests to an external HTTP pipeline.
- `SetNotSend` for marking a request as not intended to be sent by the external handler.

## Target Frameworks

This project targets `net8.0`, `net9.0`, and `net10.0`.

## Dependencies

- `Microsoft.AspNetCore.Mvc.Testing`
- `FclEx.AspNetCore`
- `FclEx.Http`

## Notes

This package is intended for test projects. It is useful when a test host should handle most requests in-memory while still allowing selected calls to reach a real HTTP endpoint.
