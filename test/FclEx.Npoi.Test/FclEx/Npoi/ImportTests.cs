using System.Data;
using Xunit;

namespace FclEx.Npoi
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
