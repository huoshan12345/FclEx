namespace FclEx.Extensions;

public static class ConvertExtensions
{
    extension(Convert)
    {
        [return: NotNullIfNotNull(nameof(value))]
        public static T? ChangeType<T>(object? value)
        {
            return (T?)Convert.ChangeType(value, typeof(T));
        }
    }
}
