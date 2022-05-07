namespace System.Reflection
{
    public static class AssemblyHelper
    {
        public static readonly Assembly AssemblyOfAction = typeof(Action).Assembly;
        public static readonly Assembly AssemblyOfFunc = typeof(Func<>).Assembly;
    }
}
