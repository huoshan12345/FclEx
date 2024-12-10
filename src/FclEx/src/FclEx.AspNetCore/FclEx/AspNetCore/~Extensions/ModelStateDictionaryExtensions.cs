using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FclEx.AspNetCore;

public static class ModelStateDictionaryExtensions
{
    public static string GetFirstError(this ModelStateDictionary modelState, string defaultError = "Unknown parameter validation error occurred")
    {
        if (modelState.IsValid)
            return "";

        var error = modelState
            .Select(x => x.Value?.Errors ?? [])
            .Where(y => y.Count > 0)
            .SelectMany(m => m)
            .FirstOrDefault();

        var result = (error?.ErrorMessage, error?.Exception?.Message, defaultError).FirstNotEmpty();
        return result;
    }
}