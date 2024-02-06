namespace FclEx.NewRelic;

public class NrqlExceptionInfo
{
    public string Nrql { get; set; } = "";
    public string[] Errors { get; set; } = Array.Empty<string>();

    public static NrqlExceptionInfo From(NrqlException ex)
    {
        return new() { Nrql = ex.Nrql, Errors = ex.Errors.Select(m => m.Message).ToArray() };
    }
}