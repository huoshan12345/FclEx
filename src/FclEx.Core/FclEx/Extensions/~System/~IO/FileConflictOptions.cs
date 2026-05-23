namespace FclEx.Extensions;

/// <summary>
/// Specifies how to handle conflicts when copying or moving files.
/// </summary>
[Flags]
public enum FileConflictOptions
{
    /// <summary>
    /// Cancel the operation silently when a conflict is detected.
    /// </summary>
    Cancel = 0,

    /// <summary>
    /// Throw an <see cref="InvalidOperationException"/> if a conflict occurs.
    /// </summary>
    ThrowOnConflict = 1,

    /// <summary>
    /// Overwrite the destination file if it already exists.
    /// </summary>
    Overwrite = 1 << 1,

    /// <summary>
    /// Automatically rename the new file if a file with the same name exists.
    /// </summary>
    AutoRename = 1 << 2,

    /// <summary>
    /// If the destination file already exists and is identical to the source,
    /// do not treat it as a conflict.  
    /// - For copy operations: the copy is skipped.
    /// - For move operations: the source file is deleted.  
    /// </summary>
    IgnoreConflictIfDuplicate = 1 << 3,

    /// <summary>
    /// Default behavior: duplicates are ignored (not treated as conflicts),
    /// otherwise the new file is renamed.
    /// </summary>
    Default = IgnoreConflictIfDuplicate | AutoRename,
}