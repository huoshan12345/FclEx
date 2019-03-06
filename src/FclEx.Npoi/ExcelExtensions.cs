using System.Collections.Generic;
using System.IO;

namespace FclEx.Npoi
{
    public static class ExcelExtensions
    {
        public static byte[] ToExcelBytes<T>(this ICollection<T> dataSource, IList<IExportColumn<T>> columns)
        {
            using (var mem = new MemoryStream())
            {
                ExcelHelper.ExportExcel(dataSource, OfficeType.Office2007, "sheet1", mem, columns);
                return mem.ToArray();
            }
        }
    }
}
