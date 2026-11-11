namespace FclEx.Extensions;

public static class PhysicalAddressExtensions
{

    /// <summary>
    /// Returns a zero-allocation, read-only view of the bytes in the specified
    /// <see cref="PhysicalAddress"/> instance.
    /// </summary>
    /// <param name="obj">
    /// The <see cref="PhysicalAddress"/> instance.
    /// </param>
    /// <returns>
    /// A read-only span over the underlying address bytes, or an empty span for an empty address.
    /// No array is copied; the span keeps its backing storage alive.
    /// </returns>
    /// <remarks>Accesses private runtime storage; compatibility depends on the runtime field layout.</remarks>
    public static ReadOnlySpan<byte> AddressBytes(this PhysicalAddress obj)
    {
#if NET8_0_OR_GREATER
        return UnsafeAccessors.PhysicalAddress_Address(obj);
#else
        return FieldInfos.PhysicalAddress_Address.GetRequiredValue<byte[]>(obj);
#endif
    }

    /// <summary>
    /// Converts the specified <see cref="PhysicalAddress"/> to a formatted string.
    /// </summary>
    /// <param name="address">
    /// The <see cref="PhysicalAddress"/> instance to format.
    /// </param>
    /// <param name="separator">
    /// The separator inserted between each byte.
    /// Default is <c>":"</c>.
    /// </param>
    /// <param name="lowerCase">
    /// Indicates whether hexadecimal characters should use lowercase letters.
    /// Default is <see langword="false"/>.
    /// </param>
    /// <returns>
    /// A formatted MAC address string.
    /// </returns>
    public static string ToFormattedString(this PhysicalAddress address, string separator = ":", bool lowerCase = false)
    {
        return StringBuilder.Build(m =>
        {
            var bytes = address.AddressBytes();
            for (int i = 0; i < bytes.Length; i++)
            {
                m.Append(bytes[i].ToString(lowerCase ? "x2" : "X2"));
                if (i < bytes.Length - 1)
                    m.Append(separator);
            }
        });
    }
}
