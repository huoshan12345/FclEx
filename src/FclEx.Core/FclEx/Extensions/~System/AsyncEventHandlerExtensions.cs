namespace FclEx.Extensions;

public static partial class AsyncEventHandlerExtensions
{
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

