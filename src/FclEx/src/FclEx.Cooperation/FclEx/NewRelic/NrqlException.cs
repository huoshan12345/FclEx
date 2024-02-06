using GraphQL;

namespace FclEx.NewRelic;

public class NrqlException : Exception
{
    public NrqlException(string nrql, GraphQLError[]? errors)
        : this(nrql, errors, errors?.FirstOrDefault()?.Message)
    {
    }

    public NrqlException(string nrql, GraphQLError[]? errors, string? message) : base(message)
    {
        Nrql = nrql;
        Errors = errors ?? Array.Empty<GraphQLError>();
    }

    public string Nrql { get; }
    public GraphQLError[] Errors { get; }
}