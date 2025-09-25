namespace FclEx.Http.Tests;

[CollectionDefinition(nameof(HttpServerTestsCollection))]
public class HttpServerTestsCollection : ICollectionFixture<HttpServerFixture>;

[Collection(nameof(HttpServerTestsCollection))]
public class HttpServerTests;