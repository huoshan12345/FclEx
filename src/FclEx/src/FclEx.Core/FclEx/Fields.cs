namespace FclEx;

public static class Fields
{
    public static readonly FieldInfo Exception_Message = typeof(Exception).GetRequiredField("_message");
    public static readonly FieldInfo Exception_StackTrace = typeof(Exception).GetRequiredField("_stackTraceString");
}