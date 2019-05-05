using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FclEx.Data;
using Xunit;

namespace FclEx.Npoi.Test
{
    public class ExportTest
    {
        private class Data
        {
            public static readonly IExportColumn<Data>[] Columns =
            {
                new ExportColumn<Data>("名字", m => m.Name),
                new ExportColumn<Data>("年龄", m => m.Age),
            };

            public string Name { get; set; }
            public int Age { get; set; }
        }

        [Fact]
        public void LargeWidthColumn_Test()
        {
            var data = new Data()
            {
                Age = 1,
                Name = "反馈老师布置寒假作业没有获得金币，已告知寒假作业分三个阶段，第一阶段老师布置一次寒假作业后会随机给老师发放金币，" +
                       "第一阶段的金币已经给老师发放了，第二阶段是在2018.12.12--2019.3.8老师成功邀请一位老师布置寒假作业会获得100金币，" +
                       "第三阶段2019.3.12--2019.3.24同一个班级每有一份练习达标会获得50金币。第二阶段老师没有完成，所以没有金币，第三阶段金币还没有到发放时间"
            };

            var bytes = new[] { data }.ToExcelBytes(Data.Columns);
            File.WriteAllBytes(nameof(LargeWidthColumn_Test) + ".xlsx", bytes);
        }

        [Fact]
        public void EmptyList_Test()
        {
            var bytes = Array.Empty<Data>().ToExcelBytes(Data.Columns);
            File.WriteAllBytes(nameof(EmptyList_Test) + ".xlsx", bytes);
        }
    }
}
