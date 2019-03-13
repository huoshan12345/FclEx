using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Xunit;

namespace FclEx.Npoi.Test
{
    public class ImportTests
    {
        [Fact]
        public void DupColumnTest()
        {
            Assert.Throws<DuplicateNameException>(() => ExcelHelper.ImportExcel("./Files/dup.xlsx"));
        }
    }
}
