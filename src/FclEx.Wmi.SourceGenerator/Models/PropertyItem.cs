using System.Management;

namespace FclEx.Wmi.SourceGenerator.Models
{
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
        /// Gets or sets the description of the property
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Gets the type of the property
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="PropertyItem"/>
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="type">The type of the property</param>
        public PropertyItem(string name, CimType type)
        {
            Name = name;
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

        /// <summary>
        /// Returns the name of the property
        /// </summary>
        /// <returns>The name</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
