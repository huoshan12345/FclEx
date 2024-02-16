using System.Collections.Generic;

namespace FclEx.Wmi.SourceGenerator.Models;

/// <summary>
/// Represents a WMI class
/// </summary>
internal class ClassItem
{
    public ClassItem(string name, Qualifiers qualifiers)
    {
        Name = name;
        Qualifiers = qualifiers;
    }

    /// <summary>
    /// Gets the name of the class
    /// </summary>
    public string Name { get; }
        
    /// <summary>
    /// Gets the properties of the class
    /// </summary>
    public List<PropertyItem> Properties { get; } = [];

    /// <summary>
    /// Gets the list with the qualifiers
    /// </summary>
    public Qualifiers Qualifiers { get; }
}