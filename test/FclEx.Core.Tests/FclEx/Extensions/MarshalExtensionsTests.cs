namespace FclEx.Extensions;

public class MarshalExtensionsTests
{
    [StructLayout(LayoutKind.Sequential)]
    private sealed class BlittablePaddedValue
    {
        public byte First;
        public int Second;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NonBlittablePaddedValue
    {
        public byte First;

        [MarshalAs(UnmanagedType.Bool)]
        public bool Second;
    }

    [Fact]
    public void ToBytes_ClearNativeBuffer_Should_Zero_Padding_Not_Written_By_Marshaler()
    {
        var value = new NonBlittablePaddedValue { First = 1, Second = true };
        var bytes = Marshal.ToBytes(value, clearNativeBuffer: true);
        var secondOffset = Marshal.OffsetOf<NonBlittablePaddedValue>(nameof(NonBlittablePaddedValue.Second)).ToInt32();

        Assert.Equal(Marshal.SizeOf<NonBlittablePaddedValue>(), bytes.Length);
        Assert.Equal(1, bytes[0]);
        Assert.Equal(1, BitConverter.ToInt32(bytes, secondOffset));
        Assert.True(secondOffset > 1);
        for (var i = 1; i < secondOffset; i++)
            Assert.Equal(0, bytes[i]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToBytes_Should_Preserve_Padding_Copied_From_Blittable_Source(bool clearNativeBuffer)
    {
        var value = new BlittablePaddedValue { First = 1, Second = 2 };
        var secondOffset = Marshal.OffsetOf<BlittablePaddedValue>(nameof(BlittablePaddedValue.Second)).ToInt32();
        Assert.True(secondOffset > 1);

        // Pin the actual object so the test controls source padding rather than relying on stack contents.
        var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
        try
        {
            for (var i = 1; i < secondOffset; i++)
                Marshal.WriteByte(handle.AddrOfPinnedObject(), i, 0xA5);

            var bytes = Marshal.ToBytes(value, clearNativeBuffer);

            Assert.Equal(Marshal.SizeOf<BlittablePaddedValue>(), bytes.Length);
            Assert.Equal(1, bytes[0]);
            Assert.Equal(2, BitConverter.ToInt32(bytes, secondOffset));
            for (var i = 1; i < secondOffset; i++)
                Assert.Equal(0xA5, bytes[i]);
        }
        finally
        {
            handle.Free();
        }
    }
}
