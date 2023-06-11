namespace FclEx.Consumers;

public class Counter
{
    private int _onConsume;
    private int _onException;
    private int _onDiscard;

    public int Consume => _onConsume;
    public int Exception => _onException;
    public int Discard => _onDiscard;

    internal void IncreConsume(int value = 1)
    {
        Interlocked.Add(ref _onConsume, value);
    }
    internal void IncreException(int value = 1)
    {
        Interlocked.Add(ref _onException, value);
    }
    internal void IncreDiscard(int value = 1)
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