namespace FclEx.Tests;

public static class StringExtensions
{
    public static string ToLiteral(this string value)
    {
        return SymbolDisplay.FormatLiteral(value, false);
    }

    public static string ToLiteral(this char value)
    {
        return value.ToString().ToLiteral();
    }

    public static string WithAssemblyInfo(this string str, char separator = '_')
    {
        // used to ensure every test assembly uses unique service, such as database.
        return StringBuilderHelper.Build(m =>
        {
            if (str.IsNotEmpty())
            {
                m.Append(str);
                m.Append(separator);
            }

            if (GlobalFixture.CurrentAssembly is not { } assembly)
                throw new InvalidOperationException("The current assembly is null.");

            var assemblyName = assembly.GetName().Name;
            if (assemblyName.IsNotEmpty())
            {
                // as short as possible cause database don't like long name.
                var name = assemblyName.TrimStart("FclEx").TrimEnd("Tests").Replace(".", "").ToLower();
                if (name.IsNotEmpty())
                {
                    m.Append(name);
                    m.Append(separator);
                }
            }

            m.Append(Environment.Version.Major);
        });
    }
}