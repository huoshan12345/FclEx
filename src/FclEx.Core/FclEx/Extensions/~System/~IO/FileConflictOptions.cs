namespace FclEx.Extensions;

[Flags]
public enum FileConflictOptions
{
    Cancel = 0,
    Throw = 1,
    Overwrite = 1 << 1,
    Rename = 1 << 2,
    DeleteOnSame = 1 << 3,
    /// <summary>
    /// The value of <see cref="DeleteOnSame"/> | <see cref="Rename"/>
    /// </summary>
    Default = DeleteOnSame | Rename,
}