namespace FclEx.Serilog;

public enum ExceptionPrintOption
{
    None,

    /// <summary>
    /// Print <see cref="Exception.ToString()"/> to <see cref="Console"/>.
    /// </summary>
    SingleMessage,

    /// <summary>
    /// Print each line of <see cref="Exception.ToString()"/> to <see cref="Console"/>.
    /// </summary>
    MessagesForEachLine,
}