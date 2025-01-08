namespace Microsoft.AspNetCore.Mvc;

[AttributeUsage(AttributeTargets.Class)]
public class ControllerNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}