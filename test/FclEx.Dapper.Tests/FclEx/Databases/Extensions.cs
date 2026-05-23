namespace FclEx.Databases;

public static class Extensions
{
    public static string GetTableName(this Type type)
    {
        return type.Name + "s"; // this is convention used in this test project.
    }
}
