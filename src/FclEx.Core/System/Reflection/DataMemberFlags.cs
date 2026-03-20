namespace System.Reflection;

[Flags]
public enum DataMemberFlags
{
    None = 0,
    Declared = 1 << 0,
    Inherited = 1 << 1,
    Instance = 1 << 2,
    Static = 1 << 3,
    Public = 1 << 4,
    NonPublic = 1 << 5,
    Field = 1 << 6,
    AutoPropertyBackingField = 1 << 7,
    Property = 1 << 8,
    Indexer = 1 << 9,
    CanRead = 1 << 10,
    CanWrite = 1 << 11,
    UnsafeWrite = 1 << 12,
}