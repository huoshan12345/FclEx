namespace FclEx.Caching.Redis._Unreachable;

[CollectionDefinition(nameof(RedisUnreachableTestsCollection))]
public class RedisUnreachableTestsCollection : ICollectionFixture<RedisUnreachableTestsFixture>;

[Collection(nameof(RedisUnreachableTestsCollection))]
public class RedisUnreachableTests(RedisUnreachableTestsFixture fixture) : RedisTests(fixture);