namespace FclEx.Web;

public class FormData(Uri submitUrl)
{
    public Uri SubmitUrl { get; set; } = submitUrl;
    public UriParams Params { get; set; } = new();
}