using System.Management;
using Xunit;
using Xunit.Abstractions;
using CIMV2;

namespace FclEx.Wmi;

public class Win32DiskDriveTests(ITestOutputHelper output)
{
    [Fact]
    public void Test()
    {
        var drives = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive").Get();
        foreach (var drive in drives.OfType<ManagementBaseObject>().Take(1))
        {
            var disk = drive.ReadAs<Win32_DiskDrive>();
            output.WriteLine(disk.PNPDeviceID);
        }
    }
}