namespace FclEx.Sources;

internal static class HashExtensionsSource
{
    private static readonly string[] _usings =
    [
        "System.Security.Cryptography",
        "FclEx.Extensions",
    ];

    private static readonly string[] _types =
    [
        nameof(MD5),
        nameof(SHA1),
        nameof(SHA256),
        nameof(SHA384),
        nameof(SHA512),
    ];

    public static SourceInfo Generate()
    {
        const string @namespace = "FclEx.Extensions";
        const string className = "HashExtensions";

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine()
            .WriteUsings(_usings)
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace, true)
            .WriteLine();

        // Class declaration
        builder.WriteLine($"public static partial class {className}")
            .WriteOpeningBracket();

        /*
            public static byte[] Md5(this byte[] input)
            {
               using var md5 = MD5.Create();
               return md5.Hash(input);
            }

            public static byte[] Md5(this byte[] input, int offset, int count)
            {
               using var md5 = MD5.Create();
               return md5.Hash(input, offset, count);
            }

            public static byte[] Md5(this ArraySegment<byte> input)
            {
               using var md5 = MD5.Create();
               return md5.Hash(input);
            }
         */

        //var types = typeof(MD5).Assembly.GetExportedTypes()
        //    .Where(m => typeof(HashAlgorithm).IsAssignableFrom(m))
        //    .Where(m =>
        //    {
        //        var method = m.GetMethod(nameof(HashAlgorithm.Create), BindingFlags.Static | BindingFlags.Public, null, [], []);
        //        return method != null && method.IsDefined(typeof(ObsoleteAttribute)) == false;
        //    });

        foreach (var type in _types)
        {
            var methodName = NormalizeCryptoName(type);
            builder.WriteLine($"public static byte[] {methodName}(this byte[] input)");
            builder.WriteOpeningBracket();
            builder.WriteLine($"using var algo = {type}.Create();");
            builder.WriteLine("return algo.Hash(input);");
            builder.WriteClosingBracket();
            builder.WriteLine();

            builder.WriteLine($"public static byte[] {methodName}(this byte[] input, int offset, int count)");
            builder.WriteOpeningBracket();
            builder.WriteLine($"using var algo = {type}.Create();");
            builder.WriteLine("return algo.Hash(input, offset, count);");
            builder.WriteClosingBracket();
            builder.WriteLine();

            builder.WriteLine($"public static byte[] {methodName}(this ArraySegment<byte> input)");
            builder.WriteOpeningBracket();
            builder.WriteLine($"using var algo = {type}.Create();");
            builder.WriteLine("return algo.Hash(input);");
            builder.WriteClosingBracket();
            builder.WriteLine();
        }

        // End class declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        return ($"{className}.g.cs", str);
    }

    private static string NormalizeCryptoName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var i = 0;

        // Find the length of the leading uppercase letters
        while (i < name.Length && char.IsUpper(name[i]))
            i++;

        if (i <= 1)
            return name;

        return char.ToUpper(name[0]) +
               name[1..i].ToLowerInvariant() +
               name[i..];
    }
}
