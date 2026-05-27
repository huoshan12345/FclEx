namespace FclEx.YamlDotNet;

/// <summary>
/// Defines how <see cref="YamlMappingNodeExtensions.TrySetScalarChild(YamlMappingNode, string, string, ScalarStyle?, YamlScalarChildConflictBehavior)"/>
/// handles an existing child whose value is not a <see cref="YamlScalarNode"/>.
/// </summary>
public enum YamlScalarChildConflictBehavior
{
    /// <summary>
    /// Leaves the existing non-scalar child unchanged.
    /// </summary>
    Ignore,

    /// <summary>
    /// Replaces the existing non-scalar child with a scalar child.
    /// </summary>
    Replace,

    /// <summary>
    /// Throws an exception when the existing child is not scalar.
    /// </summary>
    Throw,
}
