# FclEx.Serilog

Serilog helpers for FclEx.

## What Is Included

- Serilog configuration helpers and service registration.
- Enrichers and structured-property helpers.
- Excluder/filter types for source, message, property, and exception filtering.
- JSON formatter and exception formatting options.
- Log event extensions and level helpers.
- Sinks for log mutation, exception formatting, Logstash, and New Relic.

## Usage Notes

- This package bridges FclEx HTTP/logging helpers with Serilog.
- Slack-specific Serilog sinks live in `FclEx.Serilog.Slack`.
- Use the formatter options to keep exception output consistent across sinks.
