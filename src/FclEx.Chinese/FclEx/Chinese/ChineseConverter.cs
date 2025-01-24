using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using FclEx.Extensions;
using FclEx.Helpers;

namespace FclEx.Chinese;

public static class ChineseConverter
{
    internal static readonly Lazy<Dictionary<string, string>> STCharacters = new(() => LoadCharacters("STCharacters.txt"));
    internal static readonly Lazy<Dictionary<string, string>> TSCharacters = new(() => LoadCharacters("TSCharacters.txt"));

    private static Dictionary<string, string> LoadCharacters(string fileName)
    {
        var lines = ResourceHelper.LoadLinesFromEmbeddedResource(typeof(ChineseConverter).Assembly, fileName);
        return lines.Select(m => m.Split('\t'))
            .Where(m => m.Length == 2)
            .ToDictionary(m => m[0], m => m[1].Split(' ')[0]);
    }

    public static string Convert(string text, ChineseConversionDirection direction, bool preferWinApi = false)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        if (Enum.IsDefined(typeof(ChineseConversionDirection), direction) == false)
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "");

        if (text == string.Empty)
            return text;

        return preferWinApi && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? ConvertByWinApi(text, direction)
            : ConvertUsingDictionary(text, direction);
    }

    public static string ToTraditional(string source, bool preferWinApi = false)
    {
        return Convert(source, ChineseConversionDirection.ToTraditional, preferWinApi);
    }

    public static string ToSimplified(string source, bool preferWinApi = false)
    {
        return Convert(source, ChineseConversionDirection.ToSimplified, preferWinApi);
    }

    internal static string ConvertByWinApi(string text, ChineseConversionDirection direction)
    {
        var dwMapFlags = direction == ChineseConversionDirection.ToTraditional
            ? LCMAP_TRADITIONAL_CHINESE
            : LCMAP_SIMPLIFIED_CHINESE;

        var dest = new string(' ', text.Length);
        var code = LCMapStringEx(ZH_CN, dwMapFlags, text, text.Length, dest, dest.Length, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

        if (code == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        return dest;
    }

    internal static string ConvertUsingDictionary(string text, ChineseConversionDirection direction)
    {
        var dic = direction == ChineseConversionDirection.ToTraditional
            ? STCharacters.Value
            : TSCharacters.Value;

        return Replace(text, dic);
    }

    private static string Replace(string text, IReadOnlyDictionary<string, string> characters)
    {
        var changed = false;
        var textElements = text.EnumerateTextElements().ToArray();
        for (var i = 0; i < textElements.Length; i++)
        {
            if (characters.TryGetValue(textElements[i], out var value) == false)
                continue;

            textElements[i] = value;
            changed = true;
        }

        if (changed == false)
            return text;

        return StringBuilderHelper.Build(m =>
        {
            foreach (var element in textElements)
            {
                m.Append(element);
            }
        });
    }

    private const uint LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
    private const uint LCMAP_TRADITIONAL_CHINESE = 0x04000000;
    private const string ZH_CN = "zh-CN";

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int LCMapStringEx(
        string lpLocaleName,
        uint dwMapFlags,
        string lpSrcStr,
        int cchSrc,
        string lpDestStr,
        int cchDest,
        IntPtr lpVersionInformation,
        IntPtr lpReserved,
        IntPtr sortHandle);
}