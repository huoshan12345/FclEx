# FclEx.Serilog.Slack

Serilog sink support for Slack.

## What Is Included

- `SlackSink` for batched log delivery to Slack.
- Logger sink configuration extensions.
- Integration with `FclEx.Serilog` formatting and `FclEx.Slack` message delivery.

## Usage Notes

- Use this package when logs should be sent to Slack through Serilog.
- General Slack API helpers live in `FclEx.Slack`.
- Configure batching and channel behavior in the sink setup used by your Serilog pipeline.
