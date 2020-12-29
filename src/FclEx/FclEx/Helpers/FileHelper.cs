using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FclEx.Helpers
{
    public static class FileHelper
    {
        private static int WriteLines(string filePath, IEnumerable<string> lines, Encoding encoding, FileMode fileMode)
        {
            using var fs = new FileStream(filePath, fileMode, FileAccess.Write);
            using var sr = new StreamWriter(fs, encoding ?? Encoding.UTF8);
            var i = 0;
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;
                sr.WriteLine(line);
                ++i;
            }
            return i;
        }

        public static int WriteLinesAppend(string filePath, IEnumerable<string> lines, Encoding encoding)
        {
            return WriteLines(filePath, lines, encoding, FileMode.OpenOrCreate);
        }

        public static int WriteLines(string filePath, IEnumerable<string> lines, Encoding encoding)
        {
            return WriteLines(filePath, lines, encoding, FileMode.Create);
        }

        public static string? FirstExistOrNull(IEnumerable<string> paths)
        {
            return paths.Touch().NotNull().FirstOrDefault(File.Exists);
        }

        public static string? LastExistOrNull(IEnumerable<string> paths)
        {
            return paths.Touch().NotNull().LastOrDefault(File.Exists);
        }
    }
}
