using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace FclEx
{
    public static class DirectoryInfoExtensions
    {
        public static DirectoryInfo TryCreate(this DirectoryInfo di)
        {
            if (!di.Exists)
                di.Create();
            return di;
        }

        public static void MoveToRecycleBin(this FileInfo fi)
        {
            if (fi.Exists)
                FileSystem.DeleteFile(fi.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
        }

        public static void MoveToRecycleBin(this DirectoryInfo di)
        {
            if (di.Exists)
                FileSystem.DeleteDirectory(di.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
        }

        public static DirectoryInfo CreateNew(this DirectoryInfo di)
        {
            di.MoveToRecycleBin();
            di.Create();
            return di;
        }
    }
}
