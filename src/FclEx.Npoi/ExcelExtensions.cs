using System.Collections.Generic;
using System.IO;
using FclEx.Data;

namespace FclEx.Npoi
{
    public static class ExcelExtensions
    {
        public static byte[] ToExcelBytes<T>(this ICollection<T> dataSource, IList<IExportColumn<T>> columns, string sheetName = "sheet1")
        {
            using (var mem = new MemoryStream())
            {
                ExcelHelper.ExportExcel(dataSource, OfficeType.Office2007, sheetName, mem, columns);
                return mem.ToArray();
            }
        }

        public static byte[] ToExcelBytes<T>(this ICollection<T> dataSource, string sheetName = "sheet1") where T : ExportModel<T>, new()
        {
            return dataSource.ToExcelBytes(ExportModel<T>.Columns, sheetName);
        }
    }
}
