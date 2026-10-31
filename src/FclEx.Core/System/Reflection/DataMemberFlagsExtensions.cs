using static System.Reflection.DataMemberFlags;

namespace System.Reflection;

public static class DataMemberFlagsExtensions
{
    private const DataMemberFlags PublicReadableInstance = Declared | Inherited | CanRead | Property | Field | Instance | Public;
    private const DataMemberFlags ReadableInstance = PublicReadableInstance | NonPublic;

    extension(DataMemberFlags)
    {
        public static DataMemberFlags PublicReadableInstance => PublicReadableInstance;
        public static DataMemberFlags ReadableInstance => ReadableInstance;
    }
}
