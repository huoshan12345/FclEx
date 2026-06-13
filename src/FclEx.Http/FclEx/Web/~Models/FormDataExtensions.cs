namespace FclEx.Web;

/// <summary>
/// Adds and replaces parameters on <see cref="FormData"/>.
/// </summary>
public static class FormDataExtensions
{
    /// <summary>
    /// Appends a submitted form parameter.
    /// </summary>
    /// <param name="form">The form data to mutate.</param>
    /// <param name="key">The parameter name. <see langword="null"/> is preserved by <see cref="UriParams"/> as a keyless value.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>The same <paramref name="form"/> instance.</returns>
    public static FormData AddParam(this FormData form, string? key, string? value)
    {
        form.Params.Add(key, value);
        return form;
    }

    /// <summary>
    /// Replaces existing values for a submitted form parameter.
    /// </summary>
    /// <param name="form">The form data to mutate.</param>
    /// <param name="key">The parameter name. <see langword="null"/> is handled by <see cref="UriParams"/>.</param>
    /// <param name="value">The value that replaces existing values for the key.</param>
    /// <returns>The same <paramref name="form"/> instance.</returns>
    public static FormData SetParam(this FormData form, string? key, string? value)
    {
        form.Params.Set(key, value);
        return form;
    }
}
