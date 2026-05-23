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

## Notes

NRQL helpers expect a valid account id and API key. Agent helpers are only meaningful when the New Relic agent is present in the running process.
