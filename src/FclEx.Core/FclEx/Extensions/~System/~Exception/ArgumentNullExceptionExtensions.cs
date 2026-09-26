namespace FclEx.Extensions;

public static class ArgumentNullExceptionExtensions
{
    extension(ArgumentNullException)
    {
#if !NET6_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
        /// <param name="argument">The pointer argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <safety>Only compares the pointer against null; the pointed-to memory is never read or written.</safety>
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
                throw new ArgumentNullException(paramName);
        }
#endif
    }
}
