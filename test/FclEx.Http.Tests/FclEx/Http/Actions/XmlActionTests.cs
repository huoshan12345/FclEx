namespace FclEx.Http.Actions;

public class XmlActionTests
{
    [Fact]
    public void GetResult_WhenPathMatches_DeserializesResultElement()
    {
        var response = HttpActionTestFixtures.CreateResponse("<root><value>42</value></root>");
        var action = new XmlIntAction { XPathValue = "/root/value" };

        var result = action.GetResult(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void GetResult_WhenPathIsNull_DeserializesRootElement()
    {
        var response = HttpActionTestFixtures.CreateResponse("<root>42</root>");
        var action = new XmlIntAction();

        var result = action.GetResult(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void GetResult_WhenTargetTypeIsString_ReturnsElementValue()
    {
        var response = HttpActionTestFixtures.CreateResponse("<root><name>fclex</name></root>");
        var action = new XmlStringAction { XPathValue = "/root/name" };

        var result = action.GetResult(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("fclex", result.Value);
    }

    [Fact]
    public void GetResult_WhenPathDoesNotMatch_ReturnsError()
    {
        var response = HttpActionTestFixtures.CreateResponse("<root><value>42</value></root>");
        var action = new XmlIntAction { XPathValue = "/root/missing" };

        var result = action.GetResult(response);

        Assert.True(result.IsError);
        Assert.Contains("/root/missing", result.Exception!.Message);
    }

    [Fact]
    public void GetResult_WhenResponseIsNotXml_ReturnsError()
    {
        var response = HttpActionTestFixtures.CreateResponse("not xml");
        var action = new XmlIntAction();

        var result = action.GetResult(response);

        Assert.True(result.IsError);
        Assert.Contains("not a valid xml", result.Exception!.Message);
    }

    [Fact]
    public void GetResult_ForUnitAction_ReturnsSuccessWithoutReadingPayloadValue()
    {
        var response = HttpActionTestFixtures.CreateResponse("<root><ok>true</ok></root>");
        var action = new UnitXmlAction();

        var result = action.GetResult(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
    }

    [Fact]
    public void CreateContext_ExposesDocumentAndSelectedElements()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var action = new XmlIntAction { XPathValue = "/root/value" };

        var result = action.CreateContext(response, "<root><value>42</value><value>43</value></root>");

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("root", result.Value!.Document.Root!.Name.LocalName);
        Assert.Equal("/root/value", result.Value.XPath);
        Assert.Equal(new[] { 42, 43 }, result.Value.ResultElements.Select(x => int.Parse(x.Value)).ToArray());
        Assert.Equal("42", result.Value.ResultElement!.Value);
    }

    [Fact]
    public void CreateContext_WhenXPathDoesNotMatch_ReturnsError()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var action = new XmlIntAction { XPathValue = "/root/missing" };

        var result = action.CreateContext(response, "<root><value>42</value></root>");

        Assert.True(result.IsError);
        Assert.Contains("/root/missing", result.Exception!.Message);
    }

    [Fact]
    public void CreateContext_WhenXPathIsNull_SelectsDocumentRoot()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var action = new XmlStringAction();

        var result = action.CreateContext(response, "<root>value</root>");

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Same(response, result.Value.Response);
        Assert.Null(result.Value.XPath);
        Assert.Equal("root", result.Value.ResultElement!.Name.LocalName);
        Assert.Equal("value", result.Value.ResultElement.Value);
    }

    [Fact]
    public void GetXml_WhenResponseStringStartsWithXmlDeclaration_ReturnsError()
    {
        var response = HttpActionTestFixtures.CreateResponse("""<?xml version="1.0"?><root />""");
        var action = new XmlStringAction();

        var result = action.GetXml(response);

        Assert.True(result.IsError);
        Assert.Contains("not a valid xml", result.Exception!.Message);
    }

    [Fact]
    public void CreateContext_UsesXPathToSelectResultElements()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var action = new XmlIntAction { XPathValue = "/root/items/item/value" };

        var result = action.CreateContext(response, "<root><items><item><value>7</value></item></items></root>");

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("/root/items/item/value", result.Value!.XPath);
        Assert.Equal("7", result.Value.ResultElement!.Value);
    }

    [Fact]
    public void CreateContext_WhenXmlParsingFails_Throws()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var action = new XmlIntAction();

        Assert.Throws<System.Xml.XmlException>(() => (object)action.CreateContext(response, "<root><value></root>"));
    }

    [Fact]
    public async Task HttpXmlAction_WhenXmlParsingThrows_IsCaughtByPipeline()
    {
        var response = HttpActionTestFixtures.CreateResponse("<root><value></root>");
        var action = new PipelineXmlAction<int>(response);

        var result = await action.ExecuteAsync();

        Assert.True(result.IsError);
        Assert.IsType<System.Xml.XmlException>(result.Exception);
    }

    [Fact]
    public async Task HttpXmlAction_WhenResponseIsValidXml_UsesDefaultPipeline()
    {
        var response = HttpActionTestFixtures.CreateResponse("<root>42</root>", HttpStatusCode.OK, HttpActionTestFixtures.Elapsed);
        var action = new PipelineXmlAction<int>(response);

        var result = await action.ExecuteAsync();

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(42, result.Value);
        Assert.True(result.Elapsed >= TimeSpan.Zero);
    }
}
