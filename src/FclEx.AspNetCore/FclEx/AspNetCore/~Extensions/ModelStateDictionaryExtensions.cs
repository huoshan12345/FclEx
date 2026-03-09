namespace FclEx.AspNetCore;

public static class ModelStateDictionaryExtensions
{
    public static MultiValueDictionary<string, string> GetErrors(this ModelStateDictionary modelState)
    {
        if (modelState.IsValid)
            return [];

        var dic = new MultiValueDictionary<string, string>();

        foreach (var (key, entry) in modelState)
        {
            foreach (var error in entry.Errors)
            {
                dic.Add(key, error.ErrorMessage);
            }
        }

        return dic;
    }
}