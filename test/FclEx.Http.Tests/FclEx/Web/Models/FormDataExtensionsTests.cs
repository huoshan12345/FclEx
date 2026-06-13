namespace FclEx.Web.Models;

public class FormDataExtensionsTests
{
    [Fact]
    public void Constructor_UsesSubmitUriDefaultGetMethodAndEmptyParams()
    {
        var uri = new Uri("https://example.com/form");

        var form = new FormData(uri);

        Assert.Same(uri, form.SubmitUri);
        Assert.Equal(HttpMethod.Get, form.Method);
        Assert.Empty(form.Params);
    }

    [Fact]
    public void Properties_WhenAssigned_ReturnAssignedValues()
    {
        var form = new FormData(new Uri("https://example.com/form"));
        var submitUri = new Uri("https://example.com/post");
        var parameters = new UriParams { { "name", "alice" } };

        form.SubmitUri = submitUri;
        form.Method = HttpMethod.Post;
        form.Params = parameters;

        Assert.Same(submitUri, form.SubmitUri);
        Assert.Equal(HttpMethod.Post, form.Method);
        Assert.Same(parameters, form.Params);
    }

    [Fact]
    public void AddParam_AddsValueAndReturnsSameForm()
    {
        var form = new FormData(new Uri("https://example.com/form"));

        var returned = form.AddParam("name", "first").AddParam("name", "second");

        Assert.Same(form, returned);
        Assert.Equal(["first", "second"], form.Params.GetValues("name"));
    }

    [Fact]
    public void SetParam_ReplacesExistingValuesAndReturnsSameForm()
    {
        var form = new FormData(new Uri("https://example.com/form"))
            .AddParam("name", "first")
            .AddParam("name", "second");

        var returned = form.SetParam("name", "third");

        Assert.Same(form, returned);
        Assert.Equal("third", form.Params["name"]);
        var values = form.Params.GetValues("name");
        Assert.NotNull(values);
        Assert.Single(values);
    }
}
