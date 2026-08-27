namespace Xunit.v3.Priority;

/// <summary>
/// Indicates the priority value which will be assigned
/// to tests in this class which don't have a <see cref="PriorityAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DefaultPriorityAttribute: Attribute
{
    public DefaultPriorityAttribute(int priority)
    {
        Priority = priority;
    }

    public int Priority { get; private set; }
}