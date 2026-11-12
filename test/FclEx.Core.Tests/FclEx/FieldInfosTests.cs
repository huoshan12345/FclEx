using System.Net.Http;
using System.Net.NetworkInformation;

namespace FclEx;

public class FieldInfosTests
{
    public static TheoryData<FieldInfo, Type, Type> Fields => new()
    {
        { FieldInfos.Exception_Message, typeof(Exception), typeof(string) },
        { FieldInfos.Exception_StackTrace, typeof(Exception), typeof(string) },
        { FieldInfos.PhysicalAddress_Address, typeof(PhysicalAddress), typeof(byte[]) },
        { FieldInfos.IPAddress_Numbers, typeof(IPAddress), typeof(ushort[]) },
        { FieldInfos.HttpMessageInvoker_Handler, typeof(HttpMessageInvoker), typeof(HttpMessageHandler) },
    };

    [Theory]
    [MemberData(nameof(Fields))]
    public void Descriptors_ResolveNonPublicInstanceFields(FieldInfo field, Type declaringType, Type fieldType)
    {
        Assert.Equal(declaringType, field.DeclaringType);
        Assert.Equal(fieldType, field.FieldType);
        Assert.False(field.IsPublic);
        Assert.False(field.IsStatic);
    }

    [Fact]
    public void ExceptionDescriptors_ReadAndWriteBaseStorage()
    {
        var exception = new Exception("before");
        FieldInfos.Exception_Message.SetValue(exception, "after");
        FieldInfos.Exception_StackTrace.SetValue(exception, "custom stack");

        Assert.Equal("after", exception.Message);
        Assert.Equal("custom stack", exception.StackTrace);
        Assert.Same(FieldInfos.Exception_Message, FieldInfos.Exception_Message);
        Assert.Same(FieldInfos.Exception_StackTrace, FieldInfos.Exception_StackTrace);
    }

    [Fact]
    public void RequestStateDescriptor_ResolvesForCurrentRuntime()
    {
        using var request = new HttpRequestMessage();
#if NET11_0_OR_GREATER
        var field = FieldInfos.HttpRequestMessage_Flags;
        Assert.True(field.FieldType.IsEnum);
#else
        var field = FieldInfos.HttpRequestMessage_SendStatus;
        Assert.Equal(typeof(int), field.FieldType);
#endif
        Assert.Equal(typeof(HttpRequestMessage), field.DeclaringType);
        Assert.Equal(0, Convert.ToInt32(field.GetValue(request)));
    }
}
