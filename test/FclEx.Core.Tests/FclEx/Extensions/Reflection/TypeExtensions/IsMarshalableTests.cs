using FclEx.TestModels;

namespace FclEx.Extensions.Reflection.TypeExtensions;

public class IsMarshalableTests
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RepeatedMarshalableFieldStruct
    {
        public MarshalableStruct First;
        public MarshalableStruct Second;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CircularFieldClass
    {
        public CircularFieldClass? Next;
    }

    [Fact]
    public void RepeatedMarshalableFieldType_ShouldNotBeTreatedAsCircularReference()
    {
        var result = typeof(RepeatedMarshalableFieldStruct).IsMarshalable(out var ex);

        Assert.True(result, ex?.ToString());
    }

    [Fact]
    public void CircularFieldType_ShouldNotBeMarshalable()
    {
        var result = typeof(CircularFieldClass).IsMarshalable(out var ex);

        Assert.False(result);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("circular referenced", ex.Message);
    }
}
