namespace FclEx.Utils;

public readonly record struct WslCommand(
    string CommandText,
    string? WorkingDirectory = null,
    bool StripCarriageReturn = true,
    Encoding? OutputEncoding = null)
{
    public static implicit operator WslCommand(string commandText)
    {
        return new(commandText);
    }
}