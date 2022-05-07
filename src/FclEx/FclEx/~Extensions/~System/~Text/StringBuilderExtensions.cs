using System;
using System.Text;

namespace FclEx
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendLineIf(this StringBuilder sb, string value, bool condition)
        {
            if (condition)
                sb.AppendLine(value);
            return sb;
        }

        public static StringBuilder AppendIf(this StringBuilder sb, string value, bool condition)
        {
            if (condition)
                sb.Append(value);
            return sb;
        }

        public static StringBuilder AppendIfNotEmpty(this StringBuilder sb, string value)
        {
            return AppendIf(sb, value, !value.IsNullOrEmpty());
        }

        public static StringBuilder AppendIf(this StringBuilder sb, Func<string> value, bool condition)
        {
            if (condition)
                sb.Append(value());
            return sb;
        }

        public static StringBuilder AppendLineIf(this StringBuilder sb, Func<string> value, bool condition)
        {
            if (condition)
                sb.AppendLine(value());
            return sb;
        }
    }
}
