namespace FclEx.Http;

[CollectionDefinition(nameof(HttpServerTestsCollection))]
public class HttpServerTestsCollection : ICollectionFixture<HttpServerFixture>;

[Collection(nameof(HttpServerTestsCollection))]
public abstract class HttpServerTests;