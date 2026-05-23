# FclEx.Http

HTTP, HTML, cookie, authentication, and web-client helpers for FclEx.

## What Is Included

- `HttpService`, `HttpClientOptions`, request handlers, retry helpers, and download/upload utilities.
- HTTP action abstractions for JSON, JSONP, XML, HTML, and HTML file workflows.
- AngleSharp helpers for parsing and querying HTML documents.
- Cookie parsing and simple cookie models.
- OAuth client-credentials token provider and authentication handler.
- MIME type lookup helpers.
- User-client abstractions for authenticated web clients and session-aware workflows.
- Form data models and helpers for web form submission.
- Source-generated `HttpClientBuilder` helper overloads.

## Target Frameworks

By default this project targets `netstandard2.0`, `net472`, `net8.0`, `net9.0`, and `net10.0`.

## Dependencies

- `Microsoft.Extensions.Http.Polly`
- `Microsoft.IdentityModel.JsonWebTokens`
- `Duende.IdentityModel`
- `AngleSharp`
- `StandardSocketsHttpHandler` for older target frameworks
- `FclEx.Logging`
- `FclEx.SourceGenerator` as an analyzer

## Notes

The action types wrap HTTP responses into `OperationResult<T>`. When implementing custom actions, keep parsing and validation errors inside the action pipeline so callers can handle them consistently.
