# FclEx.NewRelic

New Relic helpers for agent instrumentation and NerdGraph NRQL queries.

## What Is Included

- Helpers for getting the current transaction and span from the New Relic agent.
- Safe custom event recording and custom attribute helpers.
- `NewRelicClient` for calling NerdGraph.
- NRQL query helpers that return typed `NrqlResult<T>` models.
- Retry policy support for NerdGraph responses.
- `NrqlException` and serializable exception info models.
- Dependency injection registration for `NewRelicClient`.

## Target Frameworks

By default this project targets `netstandard2.0`, `net472`, `net8.0`, `net9.0`, and `net10.0`.

## Dependencies

- `GraphQL.Client`
- `GraphQL.Client.Serializer.SystemTextJson`
- `NewRelic.Agent.Api`
- `FclEx.Http`

## Notes

NRQL helpers expect a valid account id and API key. Agent helpers are only meaningful when the New Relic agent is present in the running process.
