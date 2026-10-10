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

### Testing without Slack access

`SlackSink` also accepts an existing `ISlackApiClient` and a channel name or ID.
The caller owns the client; the sink does not dispose it. This constructor allows
applications to reuse a configured client and tests to inject a mock client without
API tokens, workspace configuration, or network access.

The offline `SlackSinkUnitTests` mock `ISlackApiClient.Chat` and
`IChatApi.PostMessage` with Moq, verifying the outgoing message channel, rich-text
blocks, formatting, timestamp ordering, duplicate suppression, and send failures.
The existing explicit integration test remains available for real Slack delivery.
