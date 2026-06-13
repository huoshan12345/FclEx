# FclEx.YamlDotNet

YamlDotNet helpers, converters, and node extensions.

## What Is Included

- `YamlHelper` for creating configured serializers and deserializers.
- Serialize and deserialize option records with naming convention and type-converter settings.
- `YamlTypeConverterAttribute` for registering converters by attribute.
- YAML type converter base class and converters for string-backed FclEx value types.
- Naming convention enum and conversion helpers.
- `YamlMappingNode`, `YamlSequenceNode`, and `YamlNode` helpers for querying, adding, updating, and removing child nodes.
- Ordered-dictionary helpers for YAML mappings.
- Emitter helpers for scalar output.

## Usage Notes

- Attribute-based converter registration is opt-in.
- Pass explicit assemblies when you want deterministic converter discovery.
- Use the node extensions for direct YAML tree manipulation and `YamlHelper` for serializer/deserializer workflows.
