namespace FclEx.Web;

/// <summary>
/// Represents an HTML form and its associated data.
/// </summary>
/// <remarks>
/// This class encapsulates the core properties of an HTML form:
/// <list type="bullet">
/// <item>
/// <description>The submission destination URI (equivalent to the 'action' attribute)</description>
/// </item>
/// <item>
/// <description>The parameters/fields to be submitted with the form</description>
/// </item>
/// </list>
/// </remarks>
/// <param name="submitUri">The URI where the form data will be submitted, corresponding to the HTML form's 'action' attribute</param>
public class FormData(Uri submitUri)
{
    /// <summary>
    /// Gets or sets the destination URI where the form data will be submitted.<br/>
    /// Equivalent to the 'action' attribute in an HTML form.
    /// </summary>
    public Uri SubmitUri { get; set; } = submitUri;

    /// <summary>
    /// Gets or sets the collection of parameters (name-value pairs) 
    /// to be submitted with the form.
    /// </summary>
    public UriParams Params { get; set; } = [];
}