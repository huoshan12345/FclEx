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
}
