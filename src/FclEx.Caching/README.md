# FclEx.Caching

Cache abstractions and EasyCaching-based implementations for FclEx.

## What Is Included

- `ICache`, `ICache<T>`, and `ICacheManager` abstractions for named and typed cache access.
- `CacheManager`, `DistributedCache`, and memory-cache-backed store helpers.
- Service registration extensions for cache managers, cache options, and typed cache configuration.
- EasyCaching provider extensions for common get-or-set workflows.
- System.Text.Json serializer patches for EasyCaching payloads.
- Option objects and configurators for cache names, expiration, and provider configuration.

## Usage Notes

- Use this package when application code should depend on FclEx cache abstractions instead of a concrete provider.
- Redis-specific collection wrappers live in `FclEx.Caching.Redis`.
- The package references EasyCaching in-memory support and `Microsoft.Extensions.Caching.Memory`.
