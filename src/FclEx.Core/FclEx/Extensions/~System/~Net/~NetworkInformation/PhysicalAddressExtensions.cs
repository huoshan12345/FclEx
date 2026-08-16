namespace FclEx.Extensions;

public static class PhysicalAddressExtensions
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// Returns a reference to the internal byte array of the specified
    /// <see cref="PhysicalAddress"/> instance without creating a copy.
    /// </summary>
    /// <param name="obj">
    /// The <see cref="PhysicalAddress"/> instance.
    /// </param>
    /// <returns>
    /// A reference to the underlying MAC address byte array.
    /// </returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_address")]
    private static extern ref byte[] Address(PhysicalAddress obj);
#endif
    /// <summary>
    /// Returns the internal byte array of the specified
    /// <see cref="PhysicalAddress"/> instance.
    /// </summary>
    /// <param name="obj">
    /// The <see cref="PhysicalAddress"/> instance.
    /// </param>
    /// <returns>
    /// The underlying MAC address byte array.
    /// </returns>
    public static IReadOnlyList<byte> AddressBytes(this PhysicalAddress obj)
    {
#if NET8_0_OR_GREATER
        return Address(obj);
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
        return address.AddressBytes()
            .Select(x => x.ToString(lowerCase ? "x2" : "X2"))
            .JoinWith(separator);
    }
}
