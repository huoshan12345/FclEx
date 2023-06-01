namespace FclEx.Http;

public static class Other
{
    public static StringBuilder AppendHttpLine(this StringBuilder sb, string value)
    {
        return sb.Append(value + HttpConstants.NewLine);
    }
}