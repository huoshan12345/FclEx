namespace FclEx.Extensions;

public static partial class AsyncEventHandlerExtensions
{
    /// <summary>Returns a type-safe, read-only snapshot of the delegates in an invocation list.</summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    /// <param name="delegate">The multicast delegate whose invocation list is retrieved.</param>
    /// <returns>A read-only view over the snapshot returned by <see cref="Delegate.GetInvocationList"/>.</returns>
    /// <remarks>
    /// Subsequent additions to or removals from <paramref name="delegate"/> do not affect the returned list.
    /// The view avoids copying the snapshot array while casting each element to <typeparamref name="T"/> when accessed.
    /// Because <see cref="Delegate"/> has a parameterless instance method with the same name, callers must explicitly
    /// specify <typeparamref name="T"/> when using this extension-method syntax.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="delegate"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<T> GetInvocationList<T>(this T @delegate) where T : Delegate
    {
        Check.NotNull(@delegate);
        return new InvocationListView<T>(@delegate.GetInvocationList());
    }

    public static Task InvokeAsync<TSender>(this AsyncEventHandler<TSender> handler, TSender sender)
    {
        return handler
            .GetInvocationList<AsyncEventHandler<TSender>>()
            .Select(m => m(sender))
            .WhenAll();
    }

    private sealed class InvocationListView<T> : IReadOnlyList<T> where T : Delegate
    {
        private readonly Delegate[] _source;

        public InvocationListView(Delegate[] source) => _source = source;

        public T this[int index] => (T)_source[index];

        public int Count => _source.Length;

        public IEnumerator<T> GetEnumerator()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var d in _source)
                yield return (T)d;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

