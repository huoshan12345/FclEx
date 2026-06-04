namespace AngleSharp.Dom;

public class ElementExtensionsTests
{
    [Fact]
    public void GetFormData_Test()
    {
        const string html = """
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <title>Form with Hidden Inputs</title>
                                <meta charset="UTF-8">
                            </head>
                            <body>
                                <form action="submit.php" method="post">
                                    <input type="hidden" name="user_id" value="12345">
                                    <input type="hidden" name="token" value="abcde12345">
                                    <input type="hidden" name="referrer" value="main_page">
                            
                                    <div>
                                        <label for="username">Username:</label>
                                        <input type="text" id="username" name="username" required>
                                    </div>
                            
                                    <div>
                                        <label for="email">Email:</label>
                                        <input type="email" id="email" name="email" required>
                                    </div>
                            
                                    <input type="submit" value="Submit">
                                </form>
                            </body>
                            </html>
                            """;

        var document = HtmlHelper.Parse(html);
        var formData = document.Body.GetFormData("form", new Uri("http://www.example.com"));

        Assert.NotNull(formData);
        Assert.Equal("http://www.example.com/submit.php", formData.SubmitUri.AbsoluteUri);
        Assert.Equal(HttpMethod.Post, formData.Method);
        Assert.Equal(5, formData.Params.Count);
        Assert.Equal("12345", formData.Params["user_id"]);
        Assert.Equal("abcde12345", formData.Params["token"]);
        Assert.Equal("main_page", formData.Params["referrer"]);
        Assert.Equal("", formData.Params["username"]);
        Assert.Equal("", formData.Params["email"]);
    }

    [Fact]
    public void GetFormData_CollectsSuccessfulControlsLikeFormSubmission()
    {
        const string html = """
                            <!DOCTYPE html>
                            <html>
                            <body>
                                <form action="/submit">
                                    <input type="hidden" name="token" value="abc">
                                    <input type="text" name="user" value="alice">
                                    <input type="password" name="password" value="secret">
                                    <input type="checkbox" name="enabled" checked>
                                    <input type="checkbox" name="features" value="a" checked>
                                    <input type="checkbox" name="features" value="b">
                                    <input type="radio" name="role" value="user">
                                    <input type="radio" name="role" value="admin" checked>
                                    <input type="text" value="missing-name">
                                    <input type="text" name="disabled" value="ignored" disabled>
                                    <fieldset disabled>
                                        <input type="text" name="fieldsetDisabled" value="ignored">
                                    </fieldset>
                                    <input type="submit" name="submit" value="ignored">
                                    <input type="file" name="upload" value="ignored">
                                    <textarea name="notes">hello</textarea>
                                    <select name="country">
                                        <option value="cn">China</option>
                                        <option value="us" selected>United States</option>
                                    </select>
                                    <select name="tags" multiple>
                                        <option value="red" selected>Red</option>
                                        <option value="blue">Blue</option>
                                        <option selected>Green</option>
                                    </select>
                                    <select name="defaultChoice">
                                        <option value="disabled-first" disabled>Disabled First</option>
                                        <option value="first">First</option>
                                        <option value="second">Second</option>
                                    </select>
                                </form>
                            </body>
                            </html>
                            """;

        var document = HtmlHelper.Parse(html);
        var formData = document.Body.GetFormData("form", new Uri("https://www.example.com/page"));

        Assert.NotNull(formData);
        Assert.Equal(HttpMethod.Get, formData.Method);
        Assert.Equal("https://www.example.com/submit", formData.SubmitUri.AbsoluteUri);
        Assert.Equal("abc", formData.Params["token"]);
        Assert.Equal("alice", formData.Params["user"]);
        Assert.Equal("secret", formData.Params["password"]);
        Assert.Equal("on", formData.Params["enabled"]);
        Assert.Equal("a", formData.Params["features"]);
        Assert.Equal("admin", formData.Params["role"]);
        Assert.Equal("hello", formData.Params["notes"]);
        Assert.Equal("us", formData.Params["country"]);
        Assert.Equal(["red", "Green"], formData.Params.GetValues("tags"));
        Assert.Equal("first", formData.Params["defaultChoice"]);
        Assert.False(formData.Params.ContainsKey("disabled"));
        Assert.False(formData.Params.ContainsKey("fieldsetDisabled"));
        Assert.False(formData.Params.ContainsKey("submit"));
        Assert.False(formData.Params.ContainsKey("upload"));
    }

    [Fact]
    public void GetFormData_WhenActionIsMissing_UsesCurrentUri()
    {
        const string html = """
                            <html>
                            <body>
                                <form method="post">
                                    <input name="q" value="fclex">
                                </form>
                            </body>
                            </html>
                            """;

        var document = HtmlHelper.Parse(html);
        var formData = document.Body.GetFormData("form", new Uri("https://www.example.com/current?x=1"));

        Assert.NotNull(formData);
        Assert.Equal(HttpMethod.Post, formData.Method);
        Assert.Equal("https://www.example.com/current?x=1", formData.SubmitUri.AbsoluteUri);
        Assert.Equal("fclex", formData.Params["q"]);
    }
}
