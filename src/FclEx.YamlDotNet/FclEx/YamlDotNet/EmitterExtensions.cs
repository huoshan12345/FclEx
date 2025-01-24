namespace FclEx.YamlDotNet;

public static class EmitterExtensions
{
    public static void Emit(this IEmitter emitter, string value)
    {
        emitter.Emit(new Scalar(value));
    }
}