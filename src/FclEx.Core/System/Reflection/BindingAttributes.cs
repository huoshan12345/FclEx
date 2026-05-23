namespace System.Reflection;

public static class BindingAttributes
{
    public const BindingFlags Declared = BindingFlags.Public
                                            | BindingFlags.NonPublic
                                            | BindingFlags.Instance
                                            | BindingFlags.Static
                                            | BindingFlags.DeclaredOnly;

    public const BindingFlags DeclaredInstance = BindingFlags.Public
                                                    | BindingFlags.NonPublic
                                                    | BindingFlags.Instance
                                                    | BindingFlags.DeclaredOnly;


    public const BindingFlags DeclaredStatic = BindingFlags.Public
                                                  | BindingFlags.NonPublic
                                                  | BindingFlags.Static
                                                  | BindingFlags.DeclaredOnly;

    public const BindingFlags GetDeclaredField = Declared | BindingFlags.GetField;
}