namespace FclEx.Extensions;

public static class JsonSerializerExtensions
{
    extension(JsonSerializer)
    {
        [MethodImpl(AggressiveInlining)]
        [return: NotNullIfNotNull(nameof(obj))]
        public static T? Clone<T>(T? obj, JsonSerializerOptions? options = null)
        {
            return obj is null ? obj : obj.ToJson(options).FromJson<T>(options);
        }
    }
}