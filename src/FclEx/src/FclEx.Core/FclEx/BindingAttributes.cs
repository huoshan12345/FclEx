namespace FclEx;

public static class BindingAttributes
{
    public const BindingFlags AllDeclared = BindingFlags.Public
                                            | BindingFlags.NonPublic
                                            | BindingFlags.Instance
                                            | BindingFlags.Static
                                            | BindingFlags.DeclaredOnly;

    public const BindingFlags AllDeclaredInstance = BindingFlags.Public
                                                    | BindingFlags.NonPublic
                                                    | BindingFlags.Instance
                                                    | BindingFlags.DeclaredOnly;


    public const BindingFlags AllDeclaredStatic = BindingFlags.Public
                                                  | BindingFlags.NonPublic
                                                  | BindingFlags.Static
                                                  | BindingFlags.DeclaredOnly;
}