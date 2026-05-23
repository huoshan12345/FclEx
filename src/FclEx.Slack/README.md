# FclEx.Slack

SlackNet extensions and formatting helpers.

## What Is Included

- Service registration helpers for SlackNet and Slack HTTP integration.
- `SlackHttp`, an HTTP implementation backed by FclEx HTTP services.
- `SlackStringBuilder` and extensions for Slack markdown formatting.
- Message and block helpers for constructing Slack messages.
- Conversation lookup, history, reply, and reaction helpers.
- Table-data conversion and chunked posting helpers for Slack messages and webhooks.

## Notes

Slack API methods are rate-limited by Slack. Use the chunking helpers for large table outputs and keep retry behavior aligned with the SlackNet client configuration.
