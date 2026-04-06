namespace FclEx.Helpers;

public static class ResourceHelper
{
    private static readonly char[] _newLineChars = Environment.NewLine.ToCharArray();


    public static class Embedded
    {
        public static Stream? GetStream(Assembly assembly, string name)
        {
            var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(name));
            var stream = resourceName == null ? null : assembly.GetManifestResourceStream(resourceName);
            return stream;
        }

        public static T ReadAs<T>(Assembly assembly, string name, Func<Stream, T> func)
        {
            using var resource = GetStream(assembly, name) ?? throw new KeyNotFoundException($"Cannot find embedded resource by name '{name}'");
            return func(resource);
        }

        public static string ReadString(Assembly assembly, string resourceName, Encoding? encoding = null) => ReadAs(assembly, resourceName, s =>
        {
            using var sr = new StreamReader(s, encoding ?? Encoding.UTF8);
            return sr.ReadToEnd();
        });

        public static string[] ReadLines(Assembly assembly, string resourceName, StringSplitOptions options, Encoding? encoding = null)
        {
            return ReadString(assembly, resourceName, encoding)
                .Split(_newLineChars, options);
        }

        public static string[] ReadLines(Assembly assembly, string resourceName, SplitOptions options = SplitOptions.TrimAndRemoveEmpty, Encoding? encoding = null)
        {
            return ReadLines(assembly, resourceName, options.ToStringSplitOptions(), encoding);
        }

        public static byte[] ReadBytes(Assembly assembly, string resourceName)
            => ReadAs(assembly, resourceName, s => s.ReadAllBytes());
    }
}