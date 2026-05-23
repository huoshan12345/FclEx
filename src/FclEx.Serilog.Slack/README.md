# FclEx.Serilog.Slack

A Serilog sink for sending batched log events to Slack.

## What Is Included

- `SlackSink`, an `IBatchedLogEventSink` implementation.
- `LoggerSinkConfiguration.Slack(...)` extension for configuring the sink.
- Integration with the Slack formatting and webhook helpers from `FclEx.Slack`.

## Notes

Slack messages should be kept concise. For high-volume logs, configure Serilog level filters or FclEx Serilog excluders before sending events to Slack.
