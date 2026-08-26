namespace Json.Path.Tests;

public static class SerializerOptions
{
	public static readonly JsonSerializerOptions Default = new()
	{
		AllowTrailingCommas = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		PropertyNameCaseInsensitive = true
	};
}