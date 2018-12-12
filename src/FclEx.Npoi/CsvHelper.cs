using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FclEx.Npoi
{
    /// <summary>
    /// 提供CSV格式文件的帮助
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// 格式化字段
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string FormatField(object field)
        {
            if (field is string)
            {
                var str = field as string;
                var wapper = false;
                if (str.IndexOf('\"') != -1)
                {
                    str = str.Replace("\"", "\"\"");
                    wapper = true;
                }
                if (str.IndexOf(',') != -1)
                {
                    wapper = true;
                }
                if (str.IndexOf('\n') != -1)
                {
                    wapper = true;
                }
                return wapper ? $"\"{str}\"" : str;
            }
            return Convert.ToString(field);
        }

        private static void ExportCsv<T>(IEnumerable<T> dataSource, IStringWriter writer, IEnumerable<IExportColumn<T>> columns)
        {
            //检测
            if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (columns == null) throw new ArgumentNullException(nameof(columns));

            //变量
            var arrColumn = columns.ToArray();
            //写标题
            for (int i = 0; i < arrColumn.Length; i++)
            {
                writer.Write(FormatField(arrColumn[i].Title));
                if (i < arrColumn.Length - 1)
                {
                    writer.Write(',');
                }
            }
            writer.WriteLine();
            //写内容
            var index = 0;
            foreach (var item in dataSource)
            {
                for (int i = 0; i < arrColumn.Length; i++)
                {
                    var val = arrColumn[i].GetValue(item, index);
                    writer.Write(FormatField(val));
                    if (i < arrColumn.Length - 1)
                    {
                        writer.Write(',');
                    }
                }
                writer.WriteLine();
                index++;
            }
        }
        /// <summary>
        /// 导出CSV字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static string ExportToCsvString<T>(IEnumerable<T> dataSource, IEnumerable<IExportColumn<T>> columns)
        {
            var sb = new StringBuilder();
            var wrapper = new StringBuilderWrapper(sb);
            ExportCsv(dataSource, wrapper, columns);
            return sb.ToString();
        }
        /// <summary>
        /// 导出CSV到数据流
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource"></param>
        /// <param name="saveStream"></param>
        /// <param name="encoding"></param>
        /// <param name="columns"></param>
        public static void ExportCsv<T>(IEnumerable<T> dataSource, Stream saveStream, Encoding encoding, IEnumerable<IExportColumn<T>> columns)
        {
            var sw = new StreamWriter(saveStream, encoding);
            var wrapper = new StreamWriterWrapper(sw);
            ExportCsv(dataSource, wrapper, columns);
        }
    }
}
