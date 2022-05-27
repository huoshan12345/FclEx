namespace FclEx.Serilog
{
  public  static class AbpSerilogConstants
    {
        public const string DefaultOutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss zzz} {Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}";
    }
}
