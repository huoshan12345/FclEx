namespace FclEx.Extensions.StringExtensions;

public class CouldBeXmlDocumentTests
{
    [Theory]
    [InlineData("<root />")]
    [InlineData("  <root></root>\r\n")]
    [InlineData("\uFEFF<?xml version=\"1.0\"?><root />")]
    [InlineData("<!--leading--><root /><!--trailing-->")]
    public void CouldBeXmlDocument_AcceptsEverySupportedXmlEnvelope(string value)
    {
        Assert.True(value.CouldBeXmlDocument());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("root")]
    [InlineData("<root")]
    [InlineData("root>")]
    public void CouldBeXmlDocument_RejectsTextThatCannotBeAnXmlDocument(string? value)
    {
        Assert.False(value.CouldBeXmlDocument());
    }

    [Fact]
    public void CouldBeXmlDocument_DoesNotClaimToValidateWellFormedness()
    {
        Assert.True("<not-valid>".CouldBeXmlDocument());
    }

    [Fact]
    public void CouldBeXmlDocument_WhenHtmlErrorPageIsNotWellFormedXml_ReturnsFalse()
    {
        var path = Path.Combine(Directories.TestData.FullName, "CouldBeXmlDocument", "DiscuzSystemError.html");
        var html = File.ReadAllText(path);

        Assert.False(html.CouldBeXmlDocument());
    }

    [Theory]
    [InlineData("<?xml version='1.0'?>")]
    [InlineData("<?instruction value?>")]
    [InlineData("<!--comment-->")]
    [InlineData("<!DOCTYPE root>")]
    [InlineData("<!DOCTYPE root [<!ELEMENT root EMPTY>]>")]
    [InlineData("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>")]
    [InlineData("<?xml version='1.0'?><!--comment--><!DOCTYPE root><!--end-->")]
    [InlineData("<!-- unclosed <root />")]
    [InlineData("<?unclosed <root />")]
    [InlineData("<!DOCTYPE root [<root />")]
    [InlineData("<!DOCTYPE root SYSTEM 'unclosed><root />")]
    [InlineData("<!--comment-->text<root />")]
    [InlineData("<>")]
    [InlineData("</root>")]
    [InlineData("<![CDATA[<root />]]>")]
    [InlineData("<1root />")]
    [InlineData("< root />")]
    [InlineData("<root attribute='unclosed>")]
    [InlineData("<root\u0001 />")]
    [InlineData("<root><!--\u0001--></root>")]
    [InlineData("<root>\u0000</root>")]
    [InlineData("<root>\u000B</root>")]
    [InlineData("<root>\uFFFE</root>")]
    [InlineData("<root>\uFFFF</root>")]
    public void CouldBeXmlDocument_RejectsInvalidCharactersOrMissingRootStartTag(string text)
    {
        Assert.False(text.CouldBeXmlDocument());
    }

    [Theory]
    [InlineData("<?instruction value?><root />")]
    [InlineData("<?xml version='1.0'?>\n<!--comment--><?instruction?><root />")]
    [InlineData("<!DOCTYPE root><root />")]
    [InlineData("<!DOCTYPE root SYSTEM 'https://example.com/external.dtd'><root />")]
    [InlineData("<!DOCTYPE root [<!ELEMENT root EMPTY>]><root />")]
    [InlineData("<!DOCTYPE root [<!ENTITY value ']> <fake />'>]><root>&value;</root>")]
    [InlineData("<!DOCTYPE root [<!-- ]> --><?instruction ]> ?><!ELEMENT root EMPTY>]><root />")]
    [InlineData("<!DOCTYPE html><html />")]
    [InlineData("<根 />")]
    [InlineData("<ns:root xmlns:ns='urn:test' />")]
    [InlineData("<root attribute='>' />")]
    [InlineData("<root>\t\r\n😀</root>")]
    public void CouldBeXmlDocument_AcceptsPrologsAndValidXmlCharacters(string text)
    {
        Assert.True(text.CouldBeXmlDocument());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CouldBeXmlDocument_RejectsUnpairedSurrogates(bool highSurrogate)
    {
        var text = "<root>" + (highSurrogate ? '\uD800' : '\uDC00') + "</root>";

        Assert.False(text.CouldBeXmlDocument());
    }
}
