# FclEx.Http Review Issues

This file records the improvement suggestions from the review of `src/FclEx.Http`.

## Issues

1. [Resolved] `src/FclEx.Http/FclEx/Http/~Extensions/HttpMessageHandlerExtensions.cs`: `EnumerateInner` can loop forever when the current handler is not a `DelegatingHandler`, because `p` is never advanced or cleared. Add an `else break`.

2. [Resolved] `src/FclEx.Http/FclEx/Http/~Auth/AuthenticationHandler.cs`: after receiving `401 Unauthorized`, the handler resends the same `HttpRequestMessage`. `HttpClient` does not allow sending the same request instance twice. Clone the request, or refresh the token and let an outer retry pipeline create a new request.

3. [Resolved] `src/FclEx.Http/FclEx/Http/~Helpers/HttpClientHelper.cs`: `CreateSocketsHttpHandler` ignores all TLS certificate validation by default through `RemoteCertificateValidationCallback = (_, _, _, _) => true`. Make secure validation the default and require explicit opt-in for insecure/test scenarios.

4. [Resolved] `src/FclEx.Http/FclEx/Http/~Auth/ClientCredentialsTokenProvider.cs`: OIDC discovery disables HTTPS, endpoint, and keyset validation by default. These should be safe by default, with explicit options for test or nonstandard identity servers.

5. [Resolved] `src/FclEx.Http/FclEx/Http/~Services/HttpClientServiceBase.cs`: request retries rebuild `HttpRequestMessage` but may reuse the same `HttpContent`. Disposing the first request can dispose the content, so later retries with a body may fail. Store a content factory or buffer reusable content before retries.

6. [Resolved] `src/FclEx.Http/FclEx/Http/~Services/HttpClientServiceBase.cs`: manual redirect handling always creates a new GET request. This loses method and body semantics for `307` and `308`, and there is no maximum redirect count or loop detection.

7. [Removed] `src/FclEx.Http/FclEx/Http/~Core/HttpQualityValueList.cs`: `FindPreferred` only searches the highest weight group. If the highest weighted value is not a candidate, lower weighted valid candidates are ignored. Traverse all accepted q-values according to weight and candidate preference.

8. [Resolved] `src/FclEx.Http/FclEx/Http/~Helpers/HttpClientHelper.cs`: `ConnectCallback` reuses one `Socket` across multiple IP address attempts. A failed connect can leave the socket unusable. Create a new socket per candidate address and dispose failed sockets.

9. [Resolved] `src/FclEx.Http/FclEx/Http/~Helpers/HttpClientHelper.cs`: `ConnectCallback` creates the socket with `new Socket(SocketType.Stream, ProtocolType.Tcp)`, which may not match IPv4/IPv6 address family choices. Create sockets with the selected address family's `AddressFamily`.

10. [Resolved] `src/FclEx.Http/FclEx/Http/~Core/HttpResponseExtensions.cs`: `ReadJsonAs<T>` parses a `JsonDocument` without disposing it. Wrap the document in `using`.

11. [Resolved] `src/FclEx.Http/FclEx/Http/~Actions/JsonActionContext.cs`: `ResultTokens` is an enumerable over a `JsonDocument` and can be evaluated after the context is disposed. Materialize selected tokens during construction or make lifetime rules stricter.

12. [Resolved] `src/FclEx.Http/FclEx/Http/~Actions/JsonActionContext.cs`: `ResultToken` returns `JsonElement?`, but `FirstOrDefault()` on value-type `JsonElement` can produce a default element that is not a meaningful null. Use an explicit `TryGetResultToken(out JsonElement token)`, a cached array, or a nullable projection that distinguishes no match.

13. [Resolved] `src/FclEx.Http/FclEx/Http/~Core/HttpRequestExtensions.Header.cs`: `TrySetHeader` is named like a .NET `Try*` method but returns `HttpRequest` and only sets the header when it is missing. Rename to something like `SetHeaderIfMissing` or `SetDefaultHeader`. Kept the `Try*` name to match the Microsoft.Extensions.DependencyInjection `TryAdd*` convention and documented that it means conditional fluent mutation, not a Boolean-returning try pattern.

14. [Resolved] `src/FclEx.Http/FclEx/Http/~Core/HttpRequestExtensions.Property.cs`: `TryCharSet` currently overwrites `CharSet`, so the name does not match behavior. Either make it conditional like `TryFallbackCharSet`, or rename it.

15. [Resolved] `src/FclEx.Http/FclEx/Http/~Core/HttpRequestExtensions.Header.cs`: `AddHeaderPair` should be renamed to `AddHeaderLine` or `ParseAndAddHeader`. Its implementation splits on every separator and should split only on the first separator so values containing `:` remain intact.

16. [Resolved] `src/FclEx.Http/FclEx/Http/~Core/HttpRequestExtensions.Header.cs`: `AcceptCompress` only sets `Accept-Encoding: gzip`. Rename it to `AcceptGZip`, or make `AcceptCompress` include supported encodings such as `br`, `gzip`, and `deflate`.

17. [Removed] `src/FclEx.Http/FclEx/Http/~Extensions/HttpClientExtensions.cs`: extension methods named `SendAsync` return response strings or JSON values, which differs significantly from BCL `HttpClient.SendAsync`. Rename to `SendStringAsync`, `SendJsonAsync`, or similar, and add `CancellationToken` parameters.

18. [Resolved] `src/FclEx.Http/FclEx/Http/~Actions`: `JsonResultPath`, `HtmlResultPath`, and `XmlResultPath` use "Path" for JSON path, CSS selector, and XPath. Rename to clearer names such as `JsonPath`, `HtmlSelector`, and `XPath`.

19. [No change] `src/FclEx.Http/FclEx/RegexesExtensions.cs`: `Regexes.CallbackName => CallbackName` is ambiguous-looking. Qualify it as `RegexesExtensions.CallbackName`.

20. [Deferred] `src/FclEx.Http`: the package mixes transport, retry, AngleSharp helpers, web-client abstractions, testing helpers, cookies, authentication, and MIME lookup. Consider splitting into focused packages such as `FclEx.Http`, `FclEx.Http.Actions`, `FclEx.AngleSharp`, and `FclEx.WebClient`.

21. [Deferred] `src/FclEx.Http/FclEx/Web/Testing`: testing helpers are included in the production package. Move them to test projects or a separate testing package.

22. [Resolved] `src/FclEx.Http`: multiple public classes are simply named `Extensions`, such as `FclEx.Web.Extensions` and HTTP action `Extensions`. Rename them to more discoverable names like `UserClientServiceCollectionExtensions` and `HttpActionExtensions`.

23. [No change] `src/FclEx.Http`: directory names such as `~Core`, `~Actions`, and `~Helpers` help ordering but are unusual for readers. Consider removing `~` if ordering is not essential.

24. [Resolved] `src/FclEx.Http/FclEx/Http/~Services/HttpClientService.cs`: provider caching stores `IServiceProvider` instances with a very large max count and no visible disposal strategy on eviction. This can accumulate providers and handlers in long-running processes.

25. [No change] `src/FclEx.Http/FclEx/Http/~Services/HttpClientService.cs`: `IsPureCanceledException` classifies cancellation by exception message text, which is culture- and implementation-sensitive. Prefer cancellation token state, exception type, or Polly timeout exceptions.

26. [No change] `src/FclEx.Http/FclEx/Http/~Core/HttpResponseExtensions.cs`: `GetDownloadInfo` uses `fileName.TrimEnd(ext)`, which removes any trailing characters contained in the extension rather than removing the exact extension. Use `Path.GetFileNameWithoutExtension`. This uses FclEx's string-suffix overload, not the BCL char-set overload.

27. [Resolved] `src/FclEx.Http/FclEx/Http/~Handlers/LoggingDelegatingHandler.cs`: `_group` is assigned but unused. Either use it in log scopes/properties or remove it.

28. [Resolved] `src/FclEx.Http/FclEx/Http/~Handlers/LoggingDelegatingHandler.cs`: successful requests are logged once inside the `try` block and again in `finally`. Decide whether both logs are useful; otherwise remove one.

29. [Resolved] `src/FclEx.Http/FclEx/Http/~Auth/IAccessTokenProvider.cs` and related implementations: token requests and discovery requests do not accept a `CancellationToken`. Add cancellation support through the public API and implementation.

30. [Removed] `src/FclEx.Http/FclEx/Http/~Extensions/HttpClientExtensions.cs`: helper `SendAsync` methods do not accept a `CancellationToken`. Add overloads or parameters so callers can cancel direct `HttpClient` helper calls.

31. [Resolved] `src/FclEx.Http/FclEx/Web/~Models/FormData.cs`: `FormData` currently captures hidden inputs only and does not model form method, normal inputs, selects, textareas, or submit button behavior. Either broaden it into a more complete form model or rename it to reflect the narrower hidden-field use case.

32. [Resolved] `src/FclEx.Http/AngleSharp/Dom/ElementExtensions.cs`: `QueryId` interpolates `prefix` into a CSS selector without escaping. Prefix values containing quotes or selector syntax can break the selector or select unintended elements.

33. [Resolved] `src/FclEx.Http/FclEx/Http/~Core/HttpResponseStream.cs`: `Dispose(bool)` disposes the `HttpResponseMessage` but not the wrapped stream directly and does not call `base.Dispose(disposing)`. Make disposal explicit and conventional.

34. [Resolved] `src/FclEx.Http/FclEx/Http/~Helpers/HttpClientHelper.cs`: local function `CheckSocketConnection` is unused. Remove it or wire it into connection validation if it is still needed.

35. [No change] `src/FclEx.Http/FclEx/Http/~Core/HttpResponseExtensions.cs` and `src/FclEx.Http/FclEx/Http/~Services/HttpServiceExtensions.cs`: download helpers are split across response extensions and service extensions. Consolidate them under one module to reduce discoverability friction.

36. [No change] `src/FclEx.Http/MimeTypes/MimeTypeMap.cs`: MIME lookup is a large standalone table inside the HTTP package. Consider whether it belongs in its own package or a core MIME utility namespace if other packages need it.

37. [Resolved] `src/FclEx.Http/FclEx/Http/~Core/HttpRequestExtensions.Form.cs`: `AddFormParam<T>(this HttpRequest request, string? key, string? value)` has an unused generic type parameter. The type cannot be inferred from the arguments, so this overload is awkward or impossible to call normally. Make it non-generic, matching `AddQueryParam(this HttpRequest request, string? key, string? value)`.

38. [Resolved] `src/FclEx.Http/FclEx/Http/~Core/HttpRequestExtensions.Header.cs`: `AddHeaderLine` validates `pair` but not `separator`, and calls `separator.ToString()` even though `separator` is already a string. Add a null/empty check for `separator` and pass it directly to `Partition`.

39. [Resolved] `src/FclEx.Http/FclEx/Http/~Core/HttpRequestExtensions.Property.cs`: the parameters named `chartSet` should be `charSet` in `CharSet`, `TryCharSet`, `FallbackCharSet`, and `TryFallbackCharSet`. Parameter names are part of the public API experience and generated documentation.

40. [Resolved] `src/FclEx.Http/FclEx/Http/~Core/HttpResponseExtensions.cs`: `GetDownloadInfo` and `LastUri` assume `VisitedUris` is non-empty. A manually constructed response, a partially populated response, or a test double can throw `InvalidOperationException`. Fall back to `response.Request.GetUri()` where possible, or return/throw a clearer error.

41. [Resolved] `src/FclEx.Http/FclEx/Http/~Core/HttpResponseExtensions.cs`: `ReadJsonAs<T>` returns `OperationResult<T>`, but malformed JSON or deserialization failures can still escape as exceptions from `JsonDocument.Parse` or `JsonElement.Deserialize<T>`. Either catch JSON/deserialization exceptions and return an error result, or document that only missing paths are represented as `OperationResult` errors.

42. [Resolved] `src/FclEx.Http/FclEx/Http/~Services/HttpClientServiceBase.cs`: `ReadCookies` and redirect bookkeeping use null-forgiving access to `responseMessage.RequestMessage?.RequestUri!`. Real `HttpClient` responses usually have it, but fake handlers and unusual responses may not. Guard the value or fall back to the current request URI.

43. [Resolved] `src/FclEx.Http/FclEx/Http/~HttpContents/CompressedContent.cs`: the base constructor reads the virtual/abstract `Encoding` property before derived `GZipContent`, `DeflateContent`, or `BrotliContent` property initializers run. This can add a null `Content-Encoding` or throw during construction. Pass the encoding name into the base constructor or use a non-virtual constructor parameter.

44. [Resolved] `src/FclEx.Http/FclEx/Http/~Extensions/HttpContentExtensions.cs`: `ReadAsStreamAsync` casts `ContentLength` to `int` for `MemoryStream` capacity. A content length greater than `int.MaxValue` can overflow before any read happens. Avoid the cast or cap the initial capacity.

45. [Resolved] `src/FclEx.Http/FclEx/Http/~Extensions/CookieExtensions.cs`: `ToSimpleCookie` drops `Cookie.Path`, so a cookie round trip through `SimpleCookie` changes its scope. Preserve the path when creating `SimpleCookie`.

46. [Deferred] `src/FclEx.Http/FclEx/RegexesExtensions.cs` and `src/FclEx.Http/FclEx/Http/~Actions/DefaultJsonpAction.cs`: `CallbackContent` is a greedy, unanchored regex tied to the fixed `_callback` name. It can match too much when extra text or multiple callback calls are present, and it does not make the callback name configurable. Use an anchored parser/regex with a single captured body and align it with the callback name actually sent.

47. [Resolved] `src/FclEx.Http/FclEx/Web/~Actions/UserClientHttpAction.cs`: `EnsureSuccessStatusCode` is a non-virtual get-only property, so derived user-client HTTP actions cannot disable status enforcement even though `HttpAction<T>` exposes this as virtual behavior. Make it virtual or otherwise configurable.

48. [Resolved] `src/FclEx.Http/FclEx/Http/~Auth/ClientCredentialsTokenProvider.cs`: clients created through the `Func<HttpClient>` constructor are not disposed after discovery or token requests. This is fine for `IHttpClientFactory`, but unclear for a factory delegate that returns a new disposable client. Document ownership semantics or dispose only when the provider explicitly owns created clients.

49. [Resolved] `src/FclEx.Http/FclEx/Http/~Utils/IWebProxyEqualityComparer.cs`: `IWebProxyEqualityComparer` is a class with an `I` prefix, which conflicts with normal .NET naming expectations for interfaces. Rename it to something like `WebProxyInterfaceEqualityComparer` or `WebProxyEqualityComparerAdapter`.

50. [Resolved] `src/FclEx.Http/FclEx/Http/~Helpers/HtmlHelper.cs`: `GetMetaRefreshUrl` relies on a very narrow `<meta http-equiv="refresh" content="..."/>` regex. Valid HTML commonly uses different attribute order, single quotes, whitespace, or non-self-closing meta tags. Consider parsing with AngleSharp or broadening the extractor.
