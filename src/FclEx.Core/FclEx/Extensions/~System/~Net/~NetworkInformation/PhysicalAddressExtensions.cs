namespace FclEx.Extensions;

public static class PhysicalAddressExtensions
{
#if NET8_0_OR_GREATER
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_address")]
    public static extern ref byte[] AddressBytes(this PhysicalAddress obj);
#else
    public static byte[] AddressBytes(this PhysicalAddress obj)
    {
        return FieldInfos.PhysicalAddress_Address.GetRequiredValue<byte[]>(obj);
    }
#endif


    public static string ToFormattedString(this PhysicalAddress address, string separator = ":", bool upperCase = true)
    {
        return address.AddressBytes()
            .Select(x => x.ToString(upperCase ? "X2" : "x2"))
            .JoinWith(separator);
    }
}
