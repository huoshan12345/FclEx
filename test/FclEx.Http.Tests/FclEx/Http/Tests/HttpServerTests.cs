using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Http.Tests;

[CollectionDefinition(nameof(HttpServerTestsCollection))]
public class HttpServerTestsCollection : ICollectionFixture<HttpServerFixture>;

[Collection(nameof(HttpServerTestsCollection))]
public class HttpServerTests;