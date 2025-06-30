// ReSharper disable RedundantUsingDirective
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Reflection;

namespace FclEx.Utils;

// This class is included in FclEx.Core.SourceGenerator as an internal class to fix conflicts when using dependabot.
#if FCLEX_INTERNAL
internal
#else
public
#endif
    sealed class SourceBuilder : IDisposable
{
    private readonly StringBuilder _builder = new();
    private readonly StringWriter _stringWriter;
    private readonly IndentedTextWriter _writer;

    public SourceBuilder()
    {
        _stringWriter = new StringWriter(_builder);
        _writer = new IndentedTextWriter(_stringWriter, new string(' ', 4));

#if !NET9_0_OR_GREATER
        // Fixes a bug in IndentedTextWriter where it doesn't indent the first line for dotnet 8.0 or lower
        // because _tabsPending is not true by default.
        typeof(IndentedTextWriter)
            .GetField("_tabsPending", BindingFlags.Instance | BindingFlags.NonPublic)?
            .SetValue(_writer, true);
#endif
    }

    public void Dispose()
    {
        _stringWriter.Dispose();
        _writer.Dispose();
    }

    /// <summary>
    /// Writes the specified value as an indented line.
    /// </summary>
    public SourceBuilder WriteLine(string? value = null)
    {
        _writer.WriteLine(value);
        return this;
    }

    public string NewLine => _writer.NewLine;

    public bool EndsWith(string value)
    {
        var startIndex = _builder.Length - value.Length;
        if (startIndex < 0)
            return false;

        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] != _builder[startIndex + i])
                return false;
        }
        return true;
    }

    /// <summary>
    /// Only keep one line terminator if this ends with multiple ones.
    /// </summary>
    /// <returns></returns>
    public SourceBuilder RemoveExtraNewLines()
    {
        var newLines = NewLine + NewLine;
        while (EndsWith(newLines))
        {
            _builder.Length -= NewLine.Length;
        }
        return this;
    }

    public SourceBuilder Write(string? value = null)
    {
        _writer.Write(value);
        return this;
    }

    public SourceBuilder WriteLineNoTabs(string? value)
    {
        _writer.WriteLineNoTabs(value!);
        return this;
    }

    public SourceBuilder Indent()
    {
        _writer.Indent++;
        return this;
    }

    public SourceBuilder Unindent()
    {
        _writer.Indent--;
        return this;
    }

    public override string ToString() => _stringWriter.ToString();
}

#if FCLEX_INTERNAL
internal
#else
public
#endif
    static class SourceBuilderExtensions
{
    /// <summary>
    /// Writes an opening bracket ("{") on its own line and increases the indentation level.
    /// </summary>
    public static SourceBuilder WriteOpeningBracket(this SourceBuilder builder)
    {
        builder.WriteLine("{");
        builder.Indent();
        return builder;
    }

    /// <summary>
    /// Decreases the indentation level, and writes a closing bracket ("}") on its own line.
    /// </summary>
    public static SourceBuilder WriteClosingBracket(this SourceBuilder builder)
    {
        builder.RemoveExtraNewLines();
        builder.Unindent();
        builder.WriteLine("}");
        return builder;
    }

    /// <summary>
    /// Writes an XML documentation summary block with the given lines.
    /// Each line is HTML-escaped and terminated with a line break tag ("&lt;br/&gt;").
    /// </summary>
    public static SourceBuilder WriteSummary(this SourceBuilder builder, IEnumerable<string> lines)
    {
        builder.WriteLine("/// <summary>");
        foreach (var line in lines)
        {
            var tagText = SecurityElement.Escape(line).Trim();
            builder.Write("/// ");
            builder.Write(tagText);
            builder.WriteLine("<br/>");
        }
        builder.WriteLine("/// </summary>");

        return builder;
    }

    /// <summary>
    /// Writes a single <see langword="using" /> directive for the specified namespace.
    /// </summary>
    public static SourceBuilder WriteUsing(this SourceBuilder builder, string @using)
    {
        return builder.WriteLine($"using {@using};");
    }

    /// <summary>
    /// Writes multiple <see langword="using" /> directives, ordered alphabetically.
    /// </summary>
    public static SourceBuilder WriteUsings(this SourceBuilder builder, IEnumerable<string> usings)
    {
        foreach (var u in usings.OrderBy(s => s))
        {
            builder.WriteUsing(u);
        }
        return builder;
    }

    /// <summary>
    /// Writes the standard auto-generated file header comment.
    /// </summary>
    public static SourceBuilder WriteGeneratedHeader(this SourceBuilder builder)
    {
        return builder.WriteLine("// <auto-generated />");
    }

    /// <summary>
    /// Writes the <c>#nullable enable</c> directive.
    /// </summary>
    public static SourceBuilder WriteEnableNullable(this SourceBuilder builder)
    {
        return builder.WriteLine("#nullable enable");
    }

    /// <summary>
    /// Writes a namespace declaration.
    /// </summary>
    public static SourceBuilder WriteNamespace(this SourceBuilder builder, string @namespace)
    {
        return builder.WriteLine($"namespace {@namespace}");
    }

    /// <summary>
    /// Writes multiple lines of text as indented lines.
    /// </summary>
    public static SourceBuilder WriteLines(this SourceBuilder builder, IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            builder.WriteLine(line);
        }
        return builder;
    }

    /// <summary>
    /// Writes multiple lines of text as indented lines.
    /// </summary>
    public static SourceBuilder WriteLines(this SourceBuilder builder, string text)
    {
        var lines = text.Replace("\r\n", "\n").Split(['\r', '\n'], StringSplitOptions.None);
        return builder.WriteLines(lines);
    }

    /// <summary>
    /// Writes a conditional compilation directive for the specified symbol.
    /// </summary>
    /// <param name="builder">The SourceBuilder instance to which the directive is added.</param>
    /// <param name="symbol">The symbol to be checked in the conditional compilation.</param>
    /// <returns>The updated SourceBuilder instance.</returns>
    public static SourceBuilder WriteIf(this SourceBuilder builder, string symbol)
    {
        return builder.WriteLineNoTabs($"#if {symbol}");
    }

    /// <summary>
    /// Writes an 'else if' conditional compilation directive for the specified symbol.
    /// </summary>
    /// <param name="builder">The SourceBuilder instance to which the directive is added.</param>
    /// <param name="symbol">The symbol to be checked in the 'else if' conditional compilation.</param>
    /// <returns>The updated SourceBuilder instance.</returns>
    public static SourceBuilder WriteElif(this SourceBuilder builder, string symbol)
    {
        return builder.WriteLineNoTabs($"#elif {symbol}");
    }

    /// <summary>
    /// Writes an 'else' conditional compilation directive.
    /// </summary>
    /// <param name="builder">The SourceBuilder instance to which the directive is added.</param>
    /// <returns>The updated SourceBuilder instance.</returns>
    public static SourceBuilder WriteElse(this SourceBuilder builder)
    {
        return builder.WriteLineNoTabs("#else");
    }

    /// <summary>
    /// Writes the end of a conditional compilation directive.
    /// </summary>
    /// <param name="builder">The SourceBuilder instance to which the directive is added.</param>
    /// <returns>The updated SourceBuilder instance.</returns>
    public static SourceBuilder WriteEndIf(this SourceBuilder builder)
    {
        return builder.WriteLineNoTabs("#endif");
    }

    /// <summary>
    /// Writes a directive to define a symbol for conditional compilation.
    /// </summary>
    /// <param name="builder">The SourceBuilder instance to which the directive is added.</param>
    /// <param name="symbol">The symbol to be defined.</param>
    /// <returns>The updated SourceBuilder instance.</returns>
    public static SourceBuilder WriteDefine(this SourceBuilder builder, string symbol)
    {
        return builder.WriteLineNoTabs($"#define {symbol}");
    }
}