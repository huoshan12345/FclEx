namespace FclEx.Http;

public delegate void OnHttpFailedCode(HttpResponseMessage response, string content);

public static class HttpClientExtensions
{
    public static readonly OnHttpFailedCode ThrowOnFailedCode = (response, content) =>
    {
        var error = content.Truncate(100);
        throw new HttpRequestException(error, null, response.StatusCode);
    };

    public static readonly OnHttpFailedCode IgnoreOnFailedCode = (response, content) => { };

    public static async Task<string> SendAsync(this HttpClient httpClient, HttpRequestMessage request, OnHttpFailedCode? onFailedCode = null)
    {
        if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
        if (request == null) throw new ArgumentNullException(nameof(request));

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        var content = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode == false)
        {
            onFailedCode ??= ThrowOnFailedCode;
            onFailedCode.Invoke(response, content);
        }
        return content;
    }

    public static async Task<T?> SendAsync<T>(this HttpClient httpClient, HttpRequestMessage request,
        OnHttpFailedCode? onFailedCode = null, JsonSerializerSettings? serializerSettings = null)
    {
        var content = await httpClient.SendAsync(request, onFailedCode);
        var result = JsonConvert.DeserializeObject<T>(content, serializerSettings);
        return result;
    }
}