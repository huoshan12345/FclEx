namespace FclEx.Extensions;

public static class BindingFlagsExtensions
{
    private const BindingFlags Declared = BindingFlags.Public
                                            | BindingFlags.NonPublic
                                            | BindingFlags.Instance
                                            | BindingFlags.Static
                                            | BindingFlags.DeclaredOnly;

    private const BindingFlags DeclaredInstance = BindingFlags.Public
                                                    | BindingFlags.NonPublic
                                                    | BindingFlags.Instance
                                                    | BindingFlags.DeclaredOnly;


    private const BindingFlags DeclaredStatic = BindingFlags.Public
                                                  | BindingFlags.NonPublic
                                                  | BindingFlags.Static
                                                  | BindingFlags.DeclaredOnly;

    private const BindingFlags VisibleToDerived = BindingFlags.Public
                                            | BindingFlags.NonPublic
                                            | BindingFlags.Instance
                                            | BindingFlags.Static
                                            | BindingFlags.FlattenHierarchy;

    private const BindingFlags DeclaredNonPublic = BindingFlags.NonPublic
                                            | BindingFlags.Instance
                                            | BindingFlags.Static
                                            | BindingFlags.DeclaredOnly;

    extension(BindingFlags)
    {
        public static BindingFlags Declared => Declared;
        public static BindingFlags DeclaredInstance => DeclaredInstance;
        public static BindingFlags DeclaredStatic => DeclaredStatic;
        public static BindingFlags VisibleToDerived => VisibleToDerived;
        public static BindingFlags DeclaredNonPublic => DeclaredNonPublic;
        // public static BindingFlags ParameterlessCtor => ParameterlessCtor;
    }
}
