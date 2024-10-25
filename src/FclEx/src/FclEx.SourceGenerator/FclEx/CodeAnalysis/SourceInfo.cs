namespace FclEx.CodeAnalysis;

public record SourceInfo(bool Success, string FileName, string Text)
{
    public static implicit operator SourceInfo((string, string) pair)
    {
        return new SourceInfo(true, pair.Item1, pair.Item2);
    }

    public static readonly SourceInfo Failed = new(false, "", "");
}