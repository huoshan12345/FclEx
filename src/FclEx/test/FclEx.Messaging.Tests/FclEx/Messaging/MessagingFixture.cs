using System.Runtime.CompilerServices;

namespace FclEx.Messaging;

public class MessagingFixture : GlobalFixture
{
    public static ConnectionSettings RmqConnection { get; } = Config.GetSection("Rmq").Get<ConnectionSettings>()!;

    [ModuleInitializer]
    internal static void Initialize()
    {
        CurrentAssembly = typeof(MessagingFixture).Assembly;
    }
}