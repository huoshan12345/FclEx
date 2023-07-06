namespace FclEx;

public interface IHasInstance<out TSelf>
{
#if NET7_0_OR_GREATER
    static abstract TSelf Instance { get; }
#endif
}