using System.Linq;
using System.Runtime.InteropServices;

namespace FclEx.Test.Extensions.ByteExtensions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UnmanagedStruct
    {
        public int Number;
        public char Char;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Arr;

        public override bool Equals(object obj)
        {
            var x = this;
            return obj is UnmanagedStruct y
                    && x.Number == y.Number
                   && x.Char == y.Char
                   && (x.Arr == y.Arr
                       || (x.Arr != null
                           && y.Arr != null
                           && x.Arr.SequenceEqual(y.Arr)));
        }

        public override int GetHashCode()
        {
            var hashCode = 2001076147;
            hashCode = hashCode * -1521134295 + Number.GetHashCode();
            hashCode = hashCode * -1521134295 + Char.GetHashCode();
            hashCode = hashCode * -1521134295 + Arr.GetHashCodeSafely();
            return hashCode;
        }
    }
}
