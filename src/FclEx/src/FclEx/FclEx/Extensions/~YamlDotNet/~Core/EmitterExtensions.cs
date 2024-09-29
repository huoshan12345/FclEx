using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace FclEx.Extensions;

public static class EmitterExtensions
{
    public static void Emit(this IEmitter emitter, string value)
    {
        emitter.Emit(new Scalar(value));
    }
}