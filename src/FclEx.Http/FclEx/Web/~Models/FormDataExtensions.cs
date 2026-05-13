namespace FclEx.Web;

public static class FormDataExtensions
{
    public static FormData AddParam(this FormData form, string? key, string? value)
    {
        form.Params.Add(key, value);
        return form;
    }

    public static FormData SetParam(this FormData form, string? key, string? value)
    {
        form.Params.Set(key, value);
        return form;
    }
}
