using System;
using Dawn;

namespace FclEx
{
    public static class RandomExtensions
    {
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string NextString(this Random random, int length)
        {
            Guard.Argument(random, nameof(random)).NotNull();
            var stringChars = new char[length];
            for (var i = 0; i < stringChars.Length; ++i)
            {
                stringChars[i] = Chars[random.Next(Chars.Length)];
            }
            return new string(stringChars);
        }

        public static long NextLong(this Random random, long max = long.MaxValue)
        {
            Guard.Argument(random, nameof(random)).NotNull();
            Guard.Argument(max, nameof(max)).NotNegative();
            return (long)(random.NextDouble() * max);
        }

        public static short NextShort(this Random random, short max = short.MaxValue)
        {
            Guard.Argument(random, nameof(random)).NotNull();
            Guard.Argument(max, nameof(max)).NotNegative();
            return (short)(random.NextDouble() * max);
        }

        public static DateTime NextDateTime(this Random random, DateTime? min = null, DateTime? max = null)
        {
            Guard.Argument(random, nameof(random)).NotNull();

            if (min.HasValue && max.HasValue && min > max)
                throw new ArgumentOutOfRangeException(nameof(min), "the min value cannot be greater than the max value.");

            var minTicks = (min ?? DateTime.MinValue).Ticks;
            var maxTicks = (max ?? DateTime.MaxValue).Ticks;

            return new DateTime(minTicks + random.NextLong(maxTicks - minTicks));
        }

        public static bool NextBool(this Random random)
        {
            Guard.Argument(random, nameof(random)).NotNull();
            return random.NextDouble() >= 0.5;
        }

        public static bool IsTrueByPercentage(this Random random, int percentage)
        {
            return random.Next(0, 100) < percentage;
        }

        //private static object Next(this Random random, TypeCode typeCode)
        //{
        //    switch (typeCode)
        //    {
        //        case TypeCode.Boolean: return random.NextDouble() >= 0.5;
        //        case TypeCode.Byte: return random.NextDouble() * byte.MaxValue;
        //        case TypeCode.Char:
        //            break;
        //        case TypeCode.DateTime:
        //            break;
        //        case TypeCode.DBNull:
        //            break;
        //        case TypeCode.Decimal:
        //            break;
        //        case TypeCode.Double:
        //            break;
        //        case TypeCode.Int16:
        //            break;
        //        case TypeCode.Int32:
        //            break;
        //        case TypeCode.Int64:
        //            break;
        //        case TypeCode.Object:
        //            break;
        //        case TypeCode.SByte:
        //            break;
        //        case TypeCode.Single:
        //            break;
        //        case TypeCode.String:
        //            break;
        //        case TypeCode.UInt16:
        //            break;
        //        case TypeCode.UInt32:
        //            break;
        //        case TypeCode.UInt64:
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //public static object NextObject(this Random random, Type type)
        //{
        //    Guard.Argument(random, nameof(random)).NotNull();
        //    Guard.Argument(type, nameof(type)).NotNull();

        //    var obj = Activator.CreateInstance(type);
        //    var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        //        .Where(m => m.CanRead);

        //    foreach (var prop in props)
        //    {
        //        var typeCode = Type.GetTypeCode(prop.PropertyType);
        //        switch (typeCode)
        //        {
        //            case TypeCode.Boolean:
        //                break;
        //            case TypeCode.Byte:
        //                break;
        //            case TypeCode.Char:
        //                break;
        //            case TypeCode.DateTime:
        //                break;
        //            case TypeCode.DBNull:
        //                break;
        //            case TypeCode.Decimal:
        //                break;
        //            case TypeCode.Double:
        //                break;
        //            case TypeCode.Int16:
        //                break;
        //            case TypeCode.Int32:
        //                break;
        //            case TypeCode.Int64:
        //                break;
        //            case TypeCode.Object:
        //                break;
        //            case TypeCode.SByte:
        //                break;
        //            case TypeCode.Single:
        //                break;
        //            case TypeCode.String:
        //                break;
        //            case TypeCode.UInt16:
        //                break;
        //            case TypeCode.UInt32:
        //                break;
        //            case TypeCode.UInt64:
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //}
    }
}
