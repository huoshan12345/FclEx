namespace FclEx.Sources.Xunit;

internal class XunitSerializableSource
{
    private const int Max = 8;

    internal static SourceInfo Generate()
    {
        const string @namespace = "FclEx.Xunit";
        const string className = "XunitSerializable";

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine()
            .WriteEnableNullable()
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace)
            .WriteOpeningBracket();

        /*
            public class XunitSerializable<T1, T2> : IXunitSerializable
            {
                public T1? Value1 { get; private set; }
                public T2? Value2 { get; private set; }

                public XunitSerializable() { }

                public XunitSerializable(T1? value1, T2? value2)
                {
                    Value1 = value1;
                    Value2 = value2;
                }

                public virtual void Deserialize(IXunitSerializationInfo info)
                {
                    Value1 = info.GetValue<T1>("_value1");
                    Value2 = info.GetValue<T2>("_value2");
                }

                public virtual void Serialize(IXunitSerializationInfo info)
                {
                    info.AddValue("_value1", Value1);
                    info.AddValue("_value2", Value2);
                }

                public void Deconstruct(out T1? value1, out T2? value2)
                {
                    value1 = Value1;
                    value2 = Value2;
                }
            }
         */
        for (var i = 2; i <= Max; i++)
        {
            var types = Enumerable.Range(1, i).Select(m => $"T{m}").JoinWith(", ");

            // Class declaration
            builder.WriteLine($"public class {className}<{types}> : IXunitSerializable")
                .WriteOpeningBracket();

            // properties
            for (var j = 1; j <= i; j++)
            {
                builder.WriteLine($"public T{j}? Value{j} {{ get; private set; }}");
            }
            builder.WriteLine();

            // default constructor
            builder.WriteLine($"public {className}() {{ }}");
            builder.WriteLine();

            // parameterized constructor
            var parameters = Enumerable.Range(1, i).Select(m => $"T{m}? value{m}").JoinWith(", ");
            builder.WriteLine($"public {className}({parameters})");
            builder.WriteOpeningBracket();
            for (var j = 1; j <= i; j++)
            {
                builder.WriteLine($"Value{j} = value{j};");
            }
            builder.WriteClosingBracket();
            builder.WriteLine();

            // Deserialize method
            builder.WriteLine("public virtual void Deserialize(IXunitSerializationInfo info)");
            builder.WriteOpeningBracket();
            for (var j = 1; j <= i; j++)
            {
                builder.WriteLine($"Value{j} = info.GetValue<T{j}>(\"_value{j}\");");
            }
            builder.WriteClosingBracket();
            builder.WriteLine();

            // Serialize method
            builder.WriteLine("public virtual void Serialize(IXunitSerializationInfo info)");
            builder.WriteOpeningBracket();
            for (var j = 1; j <= i; j++)
            {
                builder.WriteLine($"info.AddValue(\"_value{j}\", Value{j});");
            }
            builder.WriteClosingBracket();
            builder.WriteLine();

            // Deconstruct method
            var outParameters = Enumerable.Range(1, i).Select(m => $"out T{m}? value{m}").JoinWith(", ");
            builder.WriteLine($"public void Deconstruct({outParameters})");
            builder.WriteOpeningBracket();
            for (var j = 1; j <= i; j++)
            {
                builder.WriteLine($"value{j} = Value{j};");
            }
            builder.WriteClosingBracket();

            // End class declaration
            builder.WriteClosingBracket();
            builder.WriteLine();
        }

        // End namespace declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        return ($"{className}.g.cs", str);
    }
}