using System.Collections.Generic;

namespace FclEx.Wmi.SourceGenerator.Models
{
    /// <summary>
    /// Represents a WMI class
    /// </summary>
    internal class ClassItem
    {
        public ClassItem(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of the class
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the description of the class
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Gets or sets the properties of the class
        /// </summary>
        public List<PropertyItem> Properties { get; } = new();

        /// <summary>
        /// Returns the name of the class
        /// </summary>
        /// <returns>The name</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
