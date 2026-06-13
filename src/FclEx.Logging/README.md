# FclEx.Logging

`Microsoft.Extensions.Logging` helpers for FclEx.

## What Is Included

- Logger factory and provider convenience helpers.
- Scoped logging-property helpers.
- Operation timing helpers.
- Null logger fallbacks and cleanup utilities.
- Service registration helpers for logging-related options.

## Usage Notes

- This package uses Microsoft logging abstractions.
- Serilog-specific helpers live in `FclEx.Serilog`.
- Use scoped properties when downstream providers need structured context for an operation.
