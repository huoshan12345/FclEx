namespace FclEx.Utils;

public class ConsumerCounter
{
    private int _onConsume;
    private int _onException;
    private int _onDiscard;

    public int Consume => _onConsume;
    public int Exception => _onException;
    public int Discard => _onDiscard;

    internal void IncrementConsume(int value = 1)
    {
        Interlocked.Add(ref _onConsume, value);
    }
    internal void IncrementException(int value = 1)
    {
        Interlocked.Add(ref _onException, value);
    }
    internal void IncrementDiscard(int value = 1)
    {
        Interlocked.Add(ref _onDiscard, value);
    }
    internal void Reset()
    {
        Interlocked.Exchange(ref _onConsume, 0);
        Interlocked.Exchange(ref _onException, 0);
        Interlocked.Exchange(ref _onDiscard, 0);
    }
}