namespace FclEx.Extensions;

/// <summary>
/// Specifies how a file transfer handles an existing destination.
/// </summary>
public enum FileConflictResolution
{
    /// <summary>Leave both files unchanged.</summary>
    Cancel,

    /// <summary>Throw the I/O exception produced by the conflicting operation.</summary>
    Throw,

    /// <summary>Replace the existing destination.</summary>
    Overwrite,

    /// <summary>Choose a new destination name by appending or incrementing a numeric suffix.</summary>
    AutoRename,
}

/// <summary>
/// Configures file copy and move operations.
/// </summary>
/// <remarks>
/// Cancellation is intentionally supplied to each asynchronous operation rather than stored here, so the same options
/// instance can be reused safely across operations with different lifetimes.
/// </remarks>
public sealed record FileTransferOptions
{
    /// <summary>Gets the default options: ignore duplicate content and otherwise automatically rename.</summary>
    public static FileTransferOptions Default { get; } = new();

    /// <summary>Gets the strategy used when the destination exists and is not an ignored duplicate.</summary>
    public FileConflictResolution ConflictResolution { get; init; } = FileConflictResolution.AutoRename;

    /// <summary>
    /// Gets whether equal source and destination content is treated as success. A duplicate copy is skipped; a duplicate
    /// move deletes the source because the destination already contains the same data.
    /// </summary>
    public bool IgnoreConflictIfDuplicate { get; init; } = true;

    /// <summary>Gets the stream buffer size used by copy operations.</summary>
    public int BufferSize { get; init; } = FileInfoExtensions.DefaultBufferSize;

    internal void ValidateForMove()
    {
        if (Enum.IsDefined(typeof(FileConflictResolution), ConflictResolution) == false)
            throw new ArgumentOutOfRangeException(nameof(ConflictResolution));
    }

    internal void ValidateForCopy()
    {
        ValidateForMove();
        if (BufferSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(BufferSize), BufferSize, "The buffer size must be positive.");
    }
}
