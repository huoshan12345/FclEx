using System.Collections.Generic;
using System.Management;

namespace FclEx.Wmi.SourceGenerator.Models;

/// <summary>
/// Represents a property of a WMI class
/// </summary>
internal class PropertyItem
{
    /// <summary>
    /// Gets the name of the property
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the list with the qualifiers
    /// </summary>
    public Qualifiers Qualifiers { get; }

    /// <summary>
    /// Gets the type of the property
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the CIM type of the property
    /// </summary>
    public CimType CimType { get; }

    /// <summary>
    /// Gets a value indicating whether the property is an array.
    /// </summary>
    public bool IsArray { get; }

    public PropertyItem(string name, CimType type, bool isArray, Qualifiers qualifiers)
    {
        Name = name;
        CimType = type;
        IsArray = isArray;
        Qualifiers = qualifiers;
        Type = GetType(type);
    }

    /// <summary>
    /// Gets the according C# type
    /// </summary>
    /// <param name="type">The original type</param>
    /// <returns>The C# type</returns>
    private static string GetType(CimType type)
    {
        return type switch
        {
            CimType.Char16 => "char",
            CimType.Real64 => "double",
            CimType.Real32 => "Single",
            CimType.SInt8 => "sbyte",
            CimType.SInt16 => "short",
            CimType.SInt32 => "int",
            CimType.SInt64 => "long",
            CimType.UInt8 => "byte",
            _ => type.ToString()
        };
    }
}