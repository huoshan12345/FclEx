using System;
using System.Data;
using FclEx.Data;

namespace FclEx.Npoi
{
    public class DataRowExportColumn : IExportColumn<DataRow>
    {
        public DataRowExportColumn(string name, string? title = null, Func<object, int, object>? funcFormatValue = null)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            this.Name = name;
            this._title = title ?? string.Empty;
            this._funcFormatValue = funcFormatValue;
        }

        public string Name { get; private set; }
        private readonly string _title;
        private readonly Func<object, int, object>? _funcFormatValue;
        public string Title => string.IsNullOrEmpty(this._title) ? this.Name : this._title;

        public object GetValue(DataRow row, int index)
        {
            var val = row[this.Name];
            return this._funcFormatValue != null ? _funcFormatValue(val, index) : val;
        }
    }
}
