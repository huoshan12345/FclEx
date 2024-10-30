namespace FclEx.Helpers;

public static class BindingFlagsHelper
{
    public const BindingFlags AllDeclared = BindingFlags.Public
                                            | BindingFlags.NonPublic
                                            | BindingFlags.Instance
                                            | BindingFlags.Static
                                            | BindingFlags.DeclaredOnly;

    public const BindingFlags AllInstance = BindingFlags.Public
                                            | BindingFlags.NonPublic
                                            | BindingFlags.Instance;
}