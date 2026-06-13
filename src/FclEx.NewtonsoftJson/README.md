# FclEx.NewtonsoftJson

Newtonsoft.Json helpers and converters for FclEx.

## What Is Included

- Serialization and deserialization helper methods.
- `JToken` extension methods.
- Converters for string-backed values, single-or-array payloads, ignored values, key/value pairs, and read/write-as-string scenarios.
- XML conversion helpers through Newtonsoft.Json.
- Contract resolver support for bypassing converters where needed.

## Usage Notes

- `System.Text.Json` helpers live in `FclEx.Core`.
- Use this package when Newtonsoft.Json-specific behavior or converters are required.
- Converter selection can affect both read and write behavior; prefer explicit serializer settings for public contracts.
