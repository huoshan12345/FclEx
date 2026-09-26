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

## Usage Notes

- The action types wrap HTTP responses into `OperationResult<T>`.
- When implementing custom actions, keep parsing and validation errors inside the action pipeline so callers can handle them consistently.
- ASP.NET Core server-side helpers live in `FclEx.AspNetCore`.

### Named Access Token Providers

Register a client-credentials provider for each authority and client identity. The overload without a name registers the default provider under the empty name.

```csharp
services.AddHttpClient(nameof(ClientCredentialsTokenProvider));

services.AddClientCredentialsTokenProvider("captcha-a", options =>
{
    options.Authority = authA.Authority;
    options.ClientId = authA.ClientId;
    options.ClientSecret = authA.ClientSecret;
});

services.AddClientCredentialsTokenProvider("captcha-b", options =>
{
    options.Authority = authB.Authority;
    options.ClientId = authB.ClientId;
    options.ClientSecret = authB.ClientSecret;
});

services.AddHttpClient("captcha-api-a")
    .AddAuthenticationHandler("captcha-a", ["captcha.read"]);

services.AddHttpClient("captcha-api-b")
    .AddAuthenticationHandler("captcha-b", ["captcha.read"]);
```

The name passed to `AddAuthenticationHandler` selects the token provider; it is independent of the `HttpClient` name. Multiple clients can share a provider name, and their handlers can request different scopes. Each provider instance retains its discovery-document and access-token caches for the lifetime of the service provider. Re-registering a name replaces that name's previous provider factory or client-credentials configuration; registrations are not merged. The handler overload without a provider name selects the default provider.

Client-credentials providers send discovery and token requests through the HTTP client named `ClientCredentialsTokenProvider`. Configure that named client if those requests need custom transport settings such as a proxy or timeout.

### HTML Context Lifetime

Use `HtmlActionContext.Document` to access the parsed document; `Element` is obsolete and can be replaced with `Document.DocumentElement`.
The default HTML action pipeline disposes the context after result conversion, including error and exception paths. Materialize values during `GetResult`; do not return document nodes or deferred queries over them.
When constructing a context or calling `CreateContext` directly, dispose each successfully created context after use. Context copies share the same document.
CSS selectors are evaluated against the document, so selectors such as `html` and `:root` can select the root element.
