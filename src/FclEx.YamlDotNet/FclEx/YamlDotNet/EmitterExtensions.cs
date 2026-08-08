namespace FclEx.YamlDotNet;

/// <summary>
/// Provides helpers for emitting YAML events.
/// </summary>
public static class EmitterExtensions
{
    /// <summary>
    /// Emits a scalar event with the specified value.
    /// </summary>
    /// <param name="emitter">The emitter that receives the scalar event.</param>
    /// <param name="scalarValue">The scalar value to emit. Passing <see langword="null"/> creates a scalar event with a null value.</param>
    public static void Emit(this IEmitter emitter, string scalarValue)
    {
        emitter.Emit(new Scalar(scalarValue));
    }
}
