namespace FclEx.Wmi.SourceGenerator;

public enum OutputType
{
    Context = 0,
    File
}

public record struct OutputOptions(OutputType OutputType, string? Folder);