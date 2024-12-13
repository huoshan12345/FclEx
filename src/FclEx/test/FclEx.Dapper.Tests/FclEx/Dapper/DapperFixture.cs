using System.Runtime.CompilerServices;

namespace FclEx.Dapper;

public class DapperFixture : EfCoreFixture
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        CurrentAssembly = typeof(DapperFixture).Assembly;
    }
}