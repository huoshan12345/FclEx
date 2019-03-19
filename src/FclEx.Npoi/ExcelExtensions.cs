using System.Collections.Generic;
using System.IO;

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
    }
}
