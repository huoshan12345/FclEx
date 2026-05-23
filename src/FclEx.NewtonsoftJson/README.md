# FclEx.NewtonsoftJson

Newtonsoft.Json helpers, converters, and extension methods.

## What Is Included

- JSON serialization helpers with configurable camel-case and null-handling behavior.
- Extensions for converting objects, strings, XML nodes, and `JToken` values.
- Safe parsing helpers for `JObject`, `JArray`, and `JToken`.
- Converters for reading single values as arrays, reading strings as objects, writing values as strings, and ignoring values.
- `FromToStringJsonConverter<T>` for types that implement string conversion contracts.
- Converter resolver support for bypassing selected converters.
- Helpers for required child values in `JToken`.

## Target Frameworks

By default this project targets `netstandard2.0`, `net472`, `net8.0`, `net9.0`, and `net10.0`.

## Dependencies

- `Newtonsoft.Json`
- `FclEx.Core`

## Notes

Some converters intentionally relax input shape, such as allowing a single value where an array is expected. Use them only for APIs whose payload shape is known to vary.
