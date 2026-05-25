namespace FclEx.YamlDotNet;

public static class EmitterExtensions
{
    public static void Emit(this IEmitter emitter, string scalarValue)
    {
        emitter.Emit(new Scalar(scalarValue));
    }
}
