namespace FclEx.Utils;

/// <summary>
/// Defines a contract for objects that can be rendered to a <see cref="StringBuilder"/>.
/// Implementing classes must provide a <see cref="Render"/> method to generate 
/// their string representation, allowing for flexible and consistent output generation.
/// </summary>
public interface IRenderable
{
    /// <summary>
    /// Renders the object to the specified <see cref="StringBuilder"/>.
    /// This method is responsible for constructing the string representation 
    /// of the object, which can then be used for display, logging, or other output purposes.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> to which the 
    /// rendered output will be appended.</param>
    void Render(StringBuilder builder);
}

public static class RenderableExtensions
{
    public static string RenderToString(this IRenderable renderable)
    {
        return StringBuilder.Build(renderable.Render);
    }
}