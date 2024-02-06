namespace FclEx.Diagnostics;

public readonly record struct WslCommand(
    string CommandText,
    string? WorkingDirectory = null,
    bool StripCarriageReturn = true)
{
    public static implicit operator WslCommand(string commandText)
    {
        return new(commandText);
    }
}