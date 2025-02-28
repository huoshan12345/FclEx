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
        Assert.Equal(3, formData.Params.Count);
        Assert.Equal("12345", formData.Params["user_id"]);
        Assert.Equal("abcde12345", formData.Params["token"]);
        Assert.Equal("main_page", formData.Params["referrer"]);
    }
}