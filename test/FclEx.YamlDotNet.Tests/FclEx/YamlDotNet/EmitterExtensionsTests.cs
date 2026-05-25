namespace FclEx.YamlDotNet;

public class EmitterExtensionsTests
{
    [Fact]
    public void Emit_EmitsScalarEventWithValue()
    {
        var emitter = new RecordingEmitter();

        emitter.Emit("value");

        var scalar = Assert.IsType<Scalar>(Assert.Single(emitter.Events));
        Assert.Equal("value", scalar.Value);
    }

    [Fact]
    public void Emit_AllowsNullScalarValue()
    {
        var emitter = new RecordingEmitter();

        EmitterExtensions.Emit(emitter, null!);

        var scalar = Assert.IsType<Scalar>(Assert.Single(emitter.Events));
        Assert.Null(scalar.Value);
    }

    private sealed class RecordingEmitter : IEmitter
    {
        public List<ParsingEvent> Events { get; } = [];

        public void Emit(ParsingEvent @event)
        {
            Events.Add(@event);
        }
    }
}
