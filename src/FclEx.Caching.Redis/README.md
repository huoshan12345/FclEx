# FclEx.Caching.Redis

Redis-backed caching extensions for FclEx.

## What Is Included

- `IRedisManager` and `RedisManager` for resolving configured Redis databases and collections.
- Typed Redis collection abstractions for lists, sets, and hashes.
- Redis collection configuration records and service registration extensions.
- EasyCaching Redis provider patches and registration helpers.
- StackExchange.Redis integration for lower-level Redis access where needed.

## Usage Notes

- This package builds on `FclEx.Caching`; register the base cache services first when your application uses both.
- Use the typed collection abstractions when code should work with Redis data structures without scattering key and provider details.
- Configure Redis connection and collection names through the provided options objects.
