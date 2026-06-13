# FclEx.NewRelic

New Relic agent and NerdGraph helpers for FclEx.

## What Is Included

- New Relic agent helper methods.
- `NewRelicClient` and extensions for NerdGraph/NRQL queries.
- NRQL response models and metadata types.
- NRQL exception types and retry policy helpers.
- Service registration extensions.

## Usage Notes

- Use this package when application code needs to report to New Relic or query NerdGraph.
- HTTP behavior is built on `FclEx.Http`.
- Configure New Relic credentials and account information outside the package.
