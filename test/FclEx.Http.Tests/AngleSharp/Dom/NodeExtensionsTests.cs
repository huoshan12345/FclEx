namespace AngleSharp.Dom;

public class NodeExtensionsTests
{
    [Fact]
    public void OwnText_FromElement_PreservesWhitespaceAndExcludesDescendantsAndComments()
    {
        var document = HtmlParser.Parse("""<div> first <span>ignored</span><!-- ignored --> last </div>""");
        INode node = document.QuerySelector("div")!;

        Assert.Equal(" first  last ", node.OwnText());
    }

    [Fact]
    public void OwnText_FromDocumentFragment_ConcatenatesOnlyDirectTextNodesInOrder()
    {
        var document = HtmlParser.Parse("<span>ignored</span>");
        var fragment = document.CreateDocumentFragment();
        fragment.AppendChild(document.CreateTextNode(" first "));
        fragment.AppendChild(document.QuerySelector("span")!);
        fragment.AppendChild(document.CreateComment("ignored"));
        fragment.AppendChild(document.CreateTextNode(" last "));
        INode node = fragment;

        Assert.Equal(" first  last ", node.OwnText());
    }

    [Fact]
    public void OwnText_FromDocument_DoesNotIncludeDescendantText()
    {
        INode node = HtmlParser.Parse("<html><body>ignored</body></html>");

        Assert.Equal("", node.OwnText());
    }

    [Fact]
    public void OwnText_FromTextNode_DoesNotReturnTheNodesOwnData()
    {
        var document = HtmlParser.Parse("");
        INode node = document.CreateTextNode("ignored");

        Assert.Equal("", node.OwnText());
    }

    [Fact]
    public void OwnText_FromEmptyFragment_ReturnsEmptyString()
    {
        var document = HtmlParser.Parse("");
        INode node = document.CreateDocumentFragment();

        Assert.Equal("", node.OwnText());
    }
}
