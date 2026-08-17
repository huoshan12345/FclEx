using FclEx.TestModels;

namespace FclEx.Extensions.Reflection.TypeExtensions;

public class IsMarshalableTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct PointerMarshalableStruct
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string? Value;
    }

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
    public void EnsureMarshalable_ShouldNotThrow_WhenTypeIsMarshalable()
    {
        typeof(MarshalableStruct).EnsureMarshalable();
    }

    [Fact]
    public void EnsureMarshalable_ShouldThrow_WhenTypeIsNotMarshalable()
    {
        Assert.Throws<ArgumentException>(() => typeof(string).EnsureMarshalable());
    }

    [Fact]
    public void IsMarshalable_ShouldRejectPointerFields_WhenTheyAreNotAllowed()
    {
        Assert.True(typeof(PointerMarshalableStruct).IsMarshalable(out _));

        var result = typeof(PointerMarshalableStruct).IsMarshalable(out var exception, allowPointerFields: false);

        Assert.False(result);
        Assert.IsType<ArgumentException>(exception);
        Assert.Throws<ArgumentException>(() => typeof(PointerMarshalableStruct).EnsureMarshalable(allowPointerFields: false));
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
