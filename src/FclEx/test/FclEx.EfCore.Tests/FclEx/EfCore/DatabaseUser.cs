using FclEx.Tests;

namespace FclEx.EfCore;

public readonly record struct DatabaseUser(string Username, string Password, string DefaultSchema)
{
    public static readonly DatabaseUser Default = new("user".WithAssemblyInfo(), "123456", "schema".WithAssemblyInfo());
}