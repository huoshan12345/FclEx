namespace FclEx.Cooperation.Tests;

public readonly record struct SlackObject(string Id, string Name);

public static class GlobalConstants
{
    public static class SlackChannelIds
    {
        public const string MonitoringTest = "C06HH0ZP7H7";
        public const string Monitoring = "C06H3HES1CK";
    }

    public static class SlackChannelNames
    {
        public const string MonitoringTest = "monitoring-test";
        public const string Monitoring = "monitoring";
    }

    public static class SlackUserGroups
    {
        public const string EdsDev = "S042QMFBE23";
        public const string WksBj = "S05BK5H4747";
    }

    public static class SlackUsers
    {
        public const string JeremyLi = "UMHN53KBJ";
    }
}