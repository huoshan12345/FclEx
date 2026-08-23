# FclEx [![LICENSE](https://img.shields.io/github/license/mashape/apistatus.svg?style=flat)](LICENSE.TXT) [![Build](https://github.com/huoshan12345/FclEx/actions/workflows/build.yml/badge.svg)](https://github.com/huoshan12345/FclEx/actions/workflows/build.yml)

FclEx means **fundamental class libraries extensions**.

It started as a set of small, practical extensions for the .NET standard libraries. Over time it also grew into a collection of focused extensions for common libraries such as `Microsoft.Extensions.*`, ASP.NET Core, Entity Framework Core, Dapper, Serilog, SlackNet, RabbitMQ, Kafka, Newtonsoft.Json, YamlDotNet, and xUnit. The `FclEx` prefix is kept across the repository for consistency.

This repository is a multi-package library collection. Each package is intentionally scoped around one area, while sharing the same conventions and core utility layer.

## Packages

|Package|Target Frameworks|NuGet|
|----|----|----|
|[FclEx.Abp](src/FclEx.Abp)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.Abp?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.Abp)|
|[FclEx.Aop](src/FclEx.Aop)|![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.Aop?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.Aop)|
|[FclEx.AspNetCore](src/FclEx.AspNetCore)|![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.AspNetCore?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.AspNetCore)|
|[FclEx.AspNetCore.Testing](src/FclEx.AspNetCore.Testing)|![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.AspNetCore.Testing?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.AspNetCore.Testing)|
|[FclEx.Caching](src/FclEx.Caching)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.Caching?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.Caching)|
|[FclEx.Caching.Redis](src/FclEx.Caching.Redis)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.Caching.Redis?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.Caching.Redis)|
|[FclEx.Core](src/FclEx.Core)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.Core?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.Core)|
|[FclEx.Dapper](src/FclEx.Dapper)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.Dapper?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.Dapper)|
|[FclEx.DependencyInjection](src/FclEx.DependencyInjection)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.DependencyInjection?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.DependencyInjection)|
|[FclEx.EfCore](src/FclEx.EfCore)|![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.EfCore?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.EfCore)|
|[FclEx.Http](src/FclEx.Http)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.Http?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.Http)|
|[FclEx.Logging](src/FclEx.Logging)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.Logging?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.Logging)|
|[FclEx.Messaging](src/FclEx.Messaging)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.Messaging?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.Messaging)|
|[FclEx.NewRelic](src/FclEx.NewRelic)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.NewRelic?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.NewRelic)|
|[FclEx.NewtonsoftJson](src/FclEx.NewtonsoftJson)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.NewtonsoftJson?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.NewtonsoftJson)|
|[FclEx.Options](src/FclEx.Options)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.Options?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.Options)|
|[FclEx.Serilog](src/FclEx.Serilog)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.Serilog?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.Serilog)|
|[FclEx.Serilog.Slack](src/FclEx.Serilog.Slack)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.Serilog.Slack?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.Serilog.Slack)|
|[FclEx.Slack](src/FclEx.Slack)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.Slack?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.Slack)|
|[FclEx.Xunit.v3](src/FclEx.Xunit.v3)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.Xunit.v3?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.Xunit.v3)|
|[FclEx.YamlDotNet](src/FclEx.YamlDotNet)|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net472](https://img.shields.io/badge/net-472-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) ![net10.0](https://img.shields.io/badge/net-10.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/FclEx.YamlDotNet?logo=nuget&label=nuget)](https://www.nuget.org/packages/FclEx.YamlDotNet)|

<details>
<summary>Package Details</summary>

|Package|Target Frameworks|
|----|----|
|[FclEx.Abp](src/FclEx.Abp)|ABP integration helpers for FclEx, including module setup, conventional registration, telemetry suppression, and AspectCore-based login retry and return-value caching interceptors.|
|[FclEx.Aop](src/FclEx.Aop)|AspectCore-based AOP helpers for dependency injection, login-and-retry interception, and return-value caching.|
|[FclEx.AspNetCore](src/FclEx.AspNetCore)|ASP.NET Core helpers for requests, sessions, model state, endpoints, authorization, request buffering, decompression, and logging context.|
|[FclEx.AspNetCore.Testing](src/FclEx.AspNetCore.Testing)|Integration-test helpers built around WebApplicationFactory, test content roots, and selective external HTTP forwarding.|
|[FclEx.Caching](src/FclEx.Caching)|Cache abstractions and EasyCaching-based implementations for FclEx, including typed cache managers, configuration helpers, and System.Text.Json serialization patches.|
|[FclEx.Caching.Redis](src/FclEx.Caching.Redis)|Redis-backed caching extensions for FclEx, including EasyCaching Redis integration, typed Redis collection wrappers, and collection configuration helpers.|
|[FclEx.Core](src/FclEx.Core)|Foundational extensions, operation results, action pipelines, domain entity contracts, collection helpers, JSON/XML helpers, and general utilities.|
|[FclEx.Dapper](src/FclEx.Dapper)|Dapper and ADO.NET helpers for CRUD operations, local transactions, dynamic parameters, SQL adapters, type handlers, and explicit column mapping.|
|[FclEx.DependencyInjection](src/FclEx.DependencyInjection)|Convenience extensions for service registration, replacement, removal, decoration, scopes, and provider lookup.|
|[FclEx.EfCore](src/FclEx.EfCore)|Entity Framework Core query, update, soft-delete, schema, SSH tunnel, and test-model helpers.|
|[FclEx.Http](src/FclEx.Http)|HTTP service helpers, request actions, response parsing, downloads/uploads, cookies, authentication, AngleSharp HTML helpers, and user-client abstractions.|
|[FclEx.Logging](src/FclEx.Logging)|Microsoft logging helpers for scoped properties, logger creation, operation timing, null fallbacks, and logging cleanup.|
|[FclEx.Messaging](src/FclEx.Messaging)|Kafka and RabbitMQ helpers for consumers, publishers, routers, message conversion, retry metadata, and messaging logs.|
|[FclEx.NewRelic](src/FclEx.NewRelic)|New Relic agent helpers and NerdGraph NRQL client support.|
|[FclEx.NewtonsoftJson](src/FclEx.NewtonsoftJson)|Newtonsoft.Json converters and helpers for flexible JSON parsing, serialization, JToken, and XML conversion.|
|[FclEx.Options](src/FclEx.Options)|Helpers for registering prebuilt, factory-created, and service-configured options.|
|[FclEx.Serilog](src/FclEx.Serilog)|Serilog configuration helpers, enrichers, filters, formatters, and sinks.|
|[FclEx.Serilog.Slack](src/FclEx.Serilog.Slack)|Serilog sink support for sending batched log events to Slack.|
|[FclEx.Slack](src/FclEx.Slack)|SlackNet registration, Slack API extensions, message builders, webhook helpers, and table-to-message formatting.|
|[FclEx.Xunit.v3](src/FclEx.Xunit.v3)|xUnit v3 variant of the FclEx test helper package.|
|[FclEx.YamlDotNet](src/FclEx.YamlDotNet)|YamlDotNet helpers, options, converters, naming conventions, and YAML node extensions.|

</details>

## How To Choose A Package

Start with [FclEx.Core](src/FclEx.Core) when you need general .NET extensions and utility types. Add one of the integration packages only when you are using the corresponding library or framework.

For example:

- Use [FclEx.DependencyInjection](src/FclEx.DependencyInjection) for registration helpers around `IServiceCollection`.
- Use [FclEx.Http](src/FclEx.Http) for reusable HTTP request/response workflows.
- Use [FclEx.AspNetCore](src/FclEx.AspNetCore) when those HTTP helpers need to live inside an ASP.NET Core app.
- Use [FclEx.Xunit](src/FclEx.Xunit) or [FclEx.Xunit.v3](src/FclEx.Xunit.v3) from test projects, depending on the xUnit generation you target.

## Repository Layout

- `src/` contains the library projects.
- `test/` contains the corresponding test projects.
- `build/` contains build and release helper scripts.
- `misc/` contains supporting projects such as benchmarks.

## Notes

The packages are small by design: most APIs are extension methods, focused helpers, or composable building blocks. Prefer referencing the package that matches the library you are extending instead of pulling in the full repository surface.
