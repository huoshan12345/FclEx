using Meziantou.Xunit.v3;

namespace FclEx.Http;

[EnableParallelization]
[CollectionDefinition(nameof(HttpServerTestsCollection))]
public class HttpServerTestsCollection : ICollectionFixture<HttpServerFixture>;

[EnableParallelization]
[Collection(nameof(HttpServerTestsCollection))]
public class HttpServerTests;