namespace FclEx.Serilog;

/// <summary>Determines when formatted exceptions include their tree index and, when present, parent index.</summary>
/// <remarks>Select one policy; values are mutually exclusive and must not be combined as flags.</remarks>
public enum ExceptionIndexPolicy
{
    /// <summary>Never include exception indexes.</summary>
    None = 0,

    /// <summary>Include indexes even for a single exception.</summary>
    Always = 1,

    /// <summary>Include indexes when the exception tree contains more than one exception, including a linear inner-exception chain.</summary>
    WhenMultiple = 2,

    /// <summary>Include indexes only when an exception has more than one child, such as an AggregateException with multiple inner exceptions.</summary>
    WhenBranched = 3,
}
