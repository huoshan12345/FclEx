namespace FclEx.Utils;

/// <summary>
/// Builds a path by combining path parts with <see cref="Path.Combine(string[])"/> semantics.
/// </summary>
/// <remarks>
/// A rooted part resets the path accumulated before it, as it does when passed to <see cref="Path.Combine(string[])"/>.
/// </remarks>
public sealed class PathBuilder
{
    /// <summary>
    /// Initializes a builder with an optional first path part.
    /// </summary>
    /// <param name="part">The first path part, or <see langword="null"/> to start empty.</param>
    /// <exception cref="ArgumentException"><paramref name="part"/> is an empty string.</exception>
    public PathBuilder(string? part = null)
    {
        if (part is not null)
            Add(part);
    }

    private readonly List<string> _parts = [];

    /// <summary>
    /// Adds a non-empty path part to the builder.
    /// </summary>
    /// <param name="part">The path part to add.</param>
    /// <returns>This builder, for chaining calls.</returns>
    /// <remarks>A rooted part resets the path accumulated before it when <see cref="Build"/> is called.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="part"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="part"/> is empty.</exception>
    public PathBuilder Add(string part)
    {
        Check.NotEmpty(part);
        _parts.Add(part);
        return this;
    }

    /// <summary>
    /// Combines all added parts into a path using <see cref="Path.Combine(string[])"/> semantics.
    /// </summary>
    /// <returns>The combined path, or an empty string if no parts were added.</returns>
    public string Build()
    {
#if NET9_0_OR_GREATER
        var parts = _parts.AsReadOnlySpan();
#else
        var parts = _parts.ToArray();
#endif
        return Path.Combine(parts);
    }

    /// <summary>
    /// Returns the path produced by <see cref="Build"/>.
    /// </summary>
    public override string ToString() => Build();
}

/// <summary>
/// Provides methods for adding multiple path parts to a <see cref="PathBuilder"/>.
/// </summary>
public static class PathBuilderExtensions
{
    /// <summary>
    /// Adds each supplied path part to the builder in enumeration order.
    /// </summary>
    /// <param name="builder">The builder to update.</param>
    /// <param name="parts">The path parts to add. An empty sequence leaves the builder unchanged.</param>
    /// <returns>The same builder, for chaining calls.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> or <paramref name="parts"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">A part in <paramref name="parts"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">A part in <paramref name="parts"/> is empty.</exception>
    /// <remarks>Parts are added as they are enumerated. If enumeration or validation fails, parts already added remain in the builder.</remarks>
    public static PathBuilder Add(this PathBuilder builder, params IEnumerable<string> parts)
    {
        Check.NotNull(builder);
        Check.NotNull(parts);
        foreach (var part in parts)
        {
            builder.Add(part);
        }
        return builder;
    }
}
