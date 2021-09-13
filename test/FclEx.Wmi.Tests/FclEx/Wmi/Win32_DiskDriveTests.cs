using System.Management;
using Xunit;
using Xunit.Abstractions;
using CIMV2;

namespace FclEx.Wmi
{
    public class Win32_DiskDriveTests
    {
        private readonly ITestOutputHelper _output;

        public Win32_DiskDriveTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test()
        {
            var drives = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive").Get();
            foreach (var drive in drives)
            {
                var disk = drive.ReadAs<Win32_DiskDrive>();
                _output.WriteLine(disk.SerialNumber);
            }
        }
    }
}
