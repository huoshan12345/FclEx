# FclEx.AspNetCore

ASP.NET Core integration helpers for the FclEx library family.

## What Is Included

- Request and response helpers for `HttpContext`, `HttpRequest`, `ISession`, endpoints, and MVC model state.
- JWT extraction helpers that parse bearer tokens into `JwtInfo`.
- Scope-based authorization attributes and authorization handler support.
- Request body buffering middleware and request decompression providers for gzip, deflate, Brotli, and zlib.
- Hosting helpers such as `UseApplicationName`.
- Logging property enrichment from ASP.NET Core request data.

## Notes

Use this package from ASP.NET Core applications that already use the FclEx HTTP, logging, or options helpers. The request-body helpers depend on buffering being enabled before the body is read.
