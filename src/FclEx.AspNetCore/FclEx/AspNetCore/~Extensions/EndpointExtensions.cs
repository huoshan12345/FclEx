namespace FclEx.AspNetCore;

public static class EndpointExtensions
{
    /// <summary>
    /// Retrieves attributes of a specified type from an endpoint's action or controller metadata.
    /// </summary>
    /// <typeparam name="T">The type of attribute to retrieve.</typeparam>
    /// <param name="endpoint">The endpoint from which to retrieve attributes.</param>
    /// <param name="combineController">
    /// When <see langword="true"/>, combines the attributes from both the action and its controller.
    /// When <see langword="false"/>, returns only the action's attributes if they exist, 
    /// or the controller's attributes if the action's attributes are absent.
    /// </param>
    /// <param name="inherit">
    /// Indicates whether to search the inheritance chain for the attributes.
    /// Defaults to <see langword="true"/>.
    /// </param>
    /// <returns>
    /// An array of attributes of the specified type. If no attributes are found, an empty array is returned.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="endpoint"/> is <see langword="null"/>.</exception>
    public static T[] GetAttributes<T>(this Endpoint endpoint, bool combineController, bool inherit = true) where T : Attribute
    {
        Check.NotNull(endpoint);

        var actionDescriptor = endpoint.Metadata.OfType<ControllerActionDescriptor>().FirstOrDefault();
        if (actionDescriptor is null)
        {
            return endpoint.Metadata.OfType<T>().ToArray(); // when it is not a controller action, just return the attributes from the endpoint metadata
        }

        var attributes = actionDescriptor.MethodInfo
            .GetCustomAttributes<T>(inherit)
            .AsArray();

        if (attributes.IsNotEmpty() && combineController == false)
            return attributes;

        // attributes is empty or combineController is true
        var controllerAttributes = actionDescriptor.ControllerTypeInfo
            .GetCustomAttributes<T>(inherit)
            .AsArray();

        // avoid creating a new array if possible
        return attributes.IsNotEmpty()
            ? attributes.Concat(controllerAttributes).ToArray()
            : controllerAttributes;
    }
}