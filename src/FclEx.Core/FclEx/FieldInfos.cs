namespace FclEx;

public static class FieldInfos
{
    public static readonly FieldInfo Exception_Message = typeof(Exception).GetRequiredField("_message");
    public static readonly FieldInfo Exception_StackTrace = typeof(Exception).GetRequiredField("_stackTraceString");

    public static readonly FieldInfo PhysicalAddress_Address = typeof(PhysicalAddress).GetRequiredField("_address");
}