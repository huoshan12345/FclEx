namespace FclEx.Web.Models;

public class FormDataExtensionsTests
{
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
