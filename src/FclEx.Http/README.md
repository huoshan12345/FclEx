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

### HTML Context Lifetime

Use `HtmlActionContext.Document` to access the parsed document; `Element` is obsolete and can be replaced with `Document.DocumentElement`.
The default HTML action pipeline disposes the context after result conversion, including error and exception paths. Materialize values during `GetResult`; do not return document nodes or deferred queries over them.
When constructing a context or calling `CreateContext` directly, dispose each successfully created context after use. Context copies share the same document.
CSS selectors are evaluated against the document, so selectors such as `html` and `:root` can select the root element.
