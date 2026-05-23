# FclEx.Serilog

Serilog integration helpers, enrichers, filters, formatters, and sinks.

## What Is Included

- `SerilogConfiguration` and fluent configuration helpers.
- Common enrichers for OS, host, assembly, and request-related context.
- Log-event excluders for source, property, message, and exception filtering.
- JSON formatter support with exception formatting options.
- Sinks for mutating log events, formatting exceptions, New Relic output, and Logstash output.
- TCP and UDP Logstash input helpers.
- Extensions for `LoggerConfiguration`, `LoggerSinkConfiguration`, Serilog `ILogger`, and Microsoft logging builders.

## Notes

Several helpers use reflection against Serilog internals to mutate or format log events. Recheck behavior when upgrading Serilog major versions.
