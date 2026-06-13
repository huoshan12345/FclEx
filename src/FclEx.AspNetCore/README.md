# FclEx.AspNetCore

ASP.NET Core helpers for FclEx applications.

## What Is Included

- Request, context, session, endpoint, action-context, model-state, and authorization extensions.
- Request buffering middleware and request-decompression providers for Brotli, Deflate, GZip, and ZLib.
- `ControllerNameAttribute` and MVC application-model convention support.
- Required-scope authorization attribute, requirement, and handlers.
- Logging context helpers for ASP.NET Core request data.
- Host and web-host builder convenience extensions.
- JWT information helpers for request and authorization flows.

## Usage Notes

- This package targets `net8.0`, `net9.0`, and `net10.0`.
- General HTTP client helpers live in `FclEx.Http`.
- Options and logging integration come through `FclEx.Options` and `FclEx.Logging`.
