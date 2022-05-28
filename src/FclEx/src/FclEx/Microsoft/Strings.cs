namespace Microsoft;

internal class Strings
{
    public const string Argument_InvalidOffLen = "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.";
    public const string Argument_AddingDuplicateWithKey = "An item with the same key has already been added. Key: {0}";
    public const string ArgumentOutOfRange_Count = "Count must be positive and count must refer to a location within the string/array/collection.";
    public const string ArgumentOutOfRange_Index = "Index was out of range. Must be non-negative and less than the size of the collection.";
    public const string ArgumentOutOfRange_ListInsert = "Index must be within the bounds of the List.";
    public const string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";
    public const string Arg_ArrayPlusOffTooSmall = "Destination array is not long enough to copy all the items in the collection. Check array index and length.";
    public const string Arg_HTCapacityOverflow = "Capacity has overflowed.";
    public const string Arg_KeyNotFoundWithKey = "The given key '{0}' was not present in the dictionary.";
    public const string CopyTo_ArgumentsTooSmall = "Destination array is not long enough to copy all the items in the collection. Check array index and length.";
    public const string Create_TValueCollectionReadOnly = "The specified TValueCollection creates collections that have IsReadOnly set to true by default. TValueCollection must be a mutable ICollection.";
    public const string InvalidOperation_ConcurrentOperationsNotSupported = "Operations that change non-concurrent collections must have exclusive access. A concurrent update was performed on this collection and corrupted its state. The collection's state is no longer correct.";
    public const string InvalidOperation_EnumEnded = "Enumeration already finished.";
    public const string InvalidOperation_EnumFailedVersion = "Collection was modified; enumeration operation may not execute.";
    public const string InvalidOperation_EnumNotStarted = "Enumeration has not started. Call MoveNext.";
    public const string InvalidOperation_EnumOpCantHappen = "Enumeration has either not started or has already finished.";
    public const string NotSupported_KeyCollectionSet = "Mutating a key collection derived from a dictionary is not allowed.";
    public const string NotSupported_ValueCollectionSet = "Mutating a value collection derived from a dictionary is not allowed.";
    public const string ReadOnly_Modification = "The collection is read-only.";
}