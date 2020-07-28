using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using FclEx.Data;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace FclEx.Npoi
{
    /// <summary>
    /// Excel读写帮助类
    /// </summary>
    public static class ExcelHelper
    {
        static ExcelHelper()
        {
            FclExStartup.Init();
        }

        /// <summary>
        /// 格式化Excel文件名，根据Excel类型，为Excel增加后缀。
        /// </summary>
        /// <param name="fileName">未格式化的文件名</param>
        /// <param name="officeType">Excel类型</param>
        /// <returns>格式化后的Excel文件名。</returns>
        public static string FormatFileName(string fileName, OfficeType officeType)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            var ext = officeType == OfficeType.Office2007 ? ".xlsx" : ".xls";
            var name = fileName;
            if (!fileName.EndsWith(ext, StringComparison.CurrentCultureIgnoreCase))
            {
                name += ext;
            }
            return name;
        }

        #region 导出

        #region 私有

        private static void SetCellValue(ICell cell, object value)
        {
            if (value == null || value == DBNull.Value)
            {
                cell.SetCellValue(string.Empty);
                return;
            }
            if (value is string)
            {
                cell.SetCellValue((string)value);
                cell.SetCellType(CellType.String);
            }
            else if (value is DateTime)
            {
                if ((DateTime)value != default(DateTime))
                {
                    cell.SetCellValue((DateTime)value);
                }
            }
            else if (value is bool)
            {
                cell.SetCellValue((bool)value);
                cell.SetCellType(CellType.Boolean);
            }
            else if (value is short || value is int || value is long || value is byte || value is decimal || value is float || value is double)
            {
                cell.SetCellValue(Convert.ToDouble(value));
                cell.SetCellType(CellType.Numeric);
            }
            else
            {
                cell.SetCellValue(value.ToString());
            }
        }
        private static IWorkbook CreateWorkbook(OfficeType excelType)
        {
            if (excelType == OfficeType.Office2007)
            {
                return new XSSFWorkbook();
            }
            return new HSSFWorkbook();
        }
        #endregion

        /// <summary>
        /// 导出Excel,如果Excel类型为Office2003，那么数据行数不能超过65535，如果超过，则会被拆分到多个工作区中。
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="dataSource">数据源</param>
        /// <param name="excelType">EXCEL格式</param>
        /// <param name="sheetName">工作区名称</param>
        /// <param name="saveStream">保存到的文件流</param>
        /// <param name="columns">导出列</param>
        public static void ExportExcel<T>(
            ICollection<T> dataSource,
            OfficeType excelType,
            string sheetName,
            Stream saveStream,
            IList<IExportColumn<T>> columns)
        {
            if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));
            if (string.IsNullOrEmpty(sheetName)) throw new ArgumentNullException(nameof(sheetName));
            if (saveStream == null) throw new ArgumentNullException(nameof(saveStream));
            if (columns == null) throw new ArgumentNullException(nameof(columns));
            //预定义
            var book = CreateWorkbook(excelType);
            var encode = Encoding.GetEncoding(936);
            //样式
            var headerCellStyle = book.CreateCellStyle();
            headerCellStyle.Alignment = HorizontalAlignment.Center;
            var headerFont = book.CreateFont();
            headerFont.FontHeightInPoints = 10;
            headerFont.IsBold = true;
            headerCellStyle.SetFont(headerFont);
            var dateStyle = book.CreateCellStyle();
            var format = book.CreateDataFormat();
            dateStyle.DataFormat = format.GetFormat(DateTimeExtensions.CommonTimeFormat);

            //Sheet计算
            int sheetDataCount;
            switch (excelType)
            {
                case OfficeType.Office2003:
                    sheetDataCount = 65534;
                    break;
                case OfficeType.Office2007:
                    sheetDataCount = 1048574;
                    break;
                default:
                    sheetDataCount = 65534;
                    break;
            }
            var sheetList = new List<ISheet>();
            if (dataSource.Count > sheetDataCount)
            {
                var sheetCount = dataSource.Count / sheetDataCount;
                if (dataSource.Count % sheetDataCount > 0)
                {
                    sheetCount++;
                }
                for (int i = 0; i < sheetCount; i++)
                {
                    sheetList.Add(book.CreateSheet(string.Format("{0}({1})", sheetName, i + 1)));
                }
            }
            else
            {
                sheetList.Add(book.CreateSheet(sheetName));
            }
            //填充数据
            var rowIndex = 0;
            var sheetIndex = 0;
            var arrColumnWidth = new int[] { };
            foreach (var row in dataSource)
            {
                var sheet = sheetList[sheetIndex];

                //写列头
                if (rowIndex == 0)
                {
                    arrColumnWidth = new int[columns.Count];
                    var headerRow = sheet.CreateRow(rowIndex);
                    for (int i = 0; i < columns.Count; i++)
                    {
                        var column = columns[i];
                        headerRow.CreateCell(i).SetCellValue(column.Title);
                        arrColumnWidth[i] = encode.GetByteCount(column.Title);
                    }
                }
                //写内容

                var rowData = sheet.CreateRow(rowIndex + 1);
                for (int i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    var cell = rowData.CreateCell(i);
                    var data = column.GetValue(row, i);
                    SetCellValue(cell, data);
                    if (data != null && data != DBNull.Value)
                    {
                        var str = data.ToString();//转换为字符串
                        var cellWidth = encode.GetByteCount(str);
                        if (cellWidth > arrColumnWidth[i])
                        {
                            arrColumnWidth[i] = cellWidth;
                        }
                        if (data is DateTime)
                        {
                            cell.CellStyle = dateStyle;
                        }
                    }
                }

                rowIndex++;
                if (rowIndex > sheetDataCount)
                {
                    //设置列样式
                    for (int i = 0; i < columns.Count; i++)
                    {
                        var firstRow = sheet.GetRow(0);
                        if (firstRow != null)
                        {
                            var cell = firstRow.GetCell(i);
                            cell.CellStyle = headerCellStyle;
                        }
                        if (i < arrColumnWidth.Length)
                        {
                            var colWidth = (arrColumnWidth[i] + 1) * 256;
                            sheet.SetColumnWidth(i, FixColumnWidth(colWidth));
                        }
                    }
                    rowIndex = 0;
                    sheetIndex++;
                }
            }
            if (sheetList.Count > 0)
            {
                //针对最后一个sheet设置样式
                var sheet = sheetList[sheetList.Count - 1];
                for (int i = 0; i < columns.Count; i++)
                {
                    var firstRow = sheet.GetRow(0);
                    if (firstRow != null)
                    {
                        var cell = firstRow.GetCell(i);
                        cell.CellStyle = headerCellStyle;
                    }
                    if (i < arrColumnWidth.Length)
                    {
                        var colWidth = (arrColumnWidth[i] + 1) * 256;
                        sheet.SetColumnWidth(i, FixColumnWidth(colWidth));
                    }
                }
            }
            book.Write(saveStream);

            int FixColumnWidth(int width)
            {
                return width < 255 * 256 ? width < 3000 ? 3000 : width : 6000;
            }
        }

        public static void ExportExcel<T>(
            ICollection<T> dataSource,
            IList<IExportColumn<T>> columns,
            string path,
            string sheetName = "sheet1",
            OfficeType excelType = OfficeType.Office2007)
        {
            using (var fs = File.OpenWrite(path))
            {
                ExportExcel(dataSource, excelType, sheetName, fs, columns);
            }
        }
        #endregion

        #region 导入

        private static object GetCellValue(ICell cell)
        {
            switch (cell.CellType)
            {
                case CellType.Blank:
                    return string.Empty;
                case CellType.String:
                    return cell.StringCellValue;
                case CellType.Boolean:
                    return cell.BooleanCellValue;
                case CellType.Formula:
                    try
                    {
                        var e = WorkbookFactory.CreateFormulaEvaluator(cell.Sheet.Workbook);
                        e.EvaluateInCell(cell);
                        return cell.ToString();
                    }
                    catch (Exception)
                    {
                        return cell.NumericCellValue;
                    }
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        return cell.DateCellValue;
                    }
                    return cell.NumericCellValue;
                default:
                    return cell.ToString();
            }
        }

        /// <summary>
        /// 导入Excel
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static DataSet ImportExcel(Stream fileStream)
        {
            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
            var ds = new DataSet();
            var book = WorkbookFactory.Create(fileStream);
            //遍历工作区
            for (int s = 0; s < book.NumberOfSheets; s++)
            {
                var sheet = book.GetSheetAt(s);
                if (sheet.LastRowNum == 0) continue;//如果一行数据都没有，说明这个Sheet也许根本就不存在啊。。谁给搞得啊。真是的。
                var dt = new DataTable(sheet.SheetName);
                //表定义
                var headerRow = sheet.GetRow(0);
                var rowCount = sheet.LastRowNum;
                var cellCount = headerRow.LastCellNum;
                for (int c = headerRow.FirstCellNum; c < cellCount; c++)
                {
                    var dc = new DataColumn(GetCellValue(headerRow.GetCell(c)).ToString());
                    dt.Columns.Add(dc);
                }
                //表数据
                for (int r = (sheet.FirstRowNum + 1); r <= rowCount; r++)
                {
                    var row = sheet.GetRow(r);
                    if (row == null) continue;
                    var dr = dt.NewRow();
                    for (int i = row.FirstCellNum; i < cellCount; i++)
                    {
                        var cell = row.GetCell(i);
                        if (cell != null)
                        {
                            dr[i] = GetCellValue(cell);
                        }
                    }
                    dt.Rows.Add(dr);
                }
                ds.Tables.Add(dt);
            }
            return ds;
        }

        public static DataSet ImportExcel(string path)
        {
            using (var fs = File.OpenRead(path))
            {
                return ImportExcel(fs);
            }
        }
        #endregion
    }
}
