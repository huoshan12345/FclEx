namespace Json.More.Tests;

public class JsonArrayTupleConverterSerializationTests
{
	private static readonly JsonSerializerOptions _options = new()
	{
		Converters = { new JsonArrayTupleConverter() }
	};

	[Fact]
	public void OneValue()
	{
		var tuple = ValueTuple.Create(1);
		const string expected = "[1]";
		var actual = JsonSerializer.Serialize(tuple, _options);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TwoValues()
	{
		var tuple = (1, "string");
		const string expected = "[1,\"string\"]";
		var actual = JsonSerializer.Serialize(tuple, _options);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ThreeValues()
	{
		var tuple = (1, "string", false);
		const string expected = "[1,\"string\",false]";
		var actual = JsonSerializer.Serialize(tuple, _options);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void FourValues()
	{
		var tuple = (1, "string", false, -4.2);
		var expected = $"[1,\"string\",false,{SerializeNumber(-4.2)}]";
		var actual = JsonSerializer.Serialize(tuple, _options);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void FiveValues()
	{
		var tuple = (1, "string", false, -4.2, "foo");
		var expected = $"[1,\"string\",false,{SerializeNumber(-4.2)},\"foo\"]";
		var actual = JsonSerializer.Serialize(tuple, _options);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void SixValues()
	{
		var tuple = (1, "string", false, -4.2, "foo", 6);
		var expected = $"[1,\"string\",false,{SerializeNumber(-4.2)},\"foo\",6]";
		var actual = JsonSerializer.Serialize(tuple, _options);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void SevenValues()
	{
		var tuple = (1, "string", false, -4.2, "foo", 6, 7);
		var expected = $"[1,\"string\",false,{SerializeNumber(-4.2)},\"foo\",6,7]";
		var actual = JsonSerializer.Serialize(tuple, _options);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void EightValues()
	{
		var tuple = (1, "string", false, -4.2, "foo", 6, 7, 8);
		var expected = $"[1,\"string\",false,{SerializeNumber(-4.2)},\"foo\",6,7,8]";
		var actual = JsonSerializer.Serialize(tuple, _options);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void MoreValues()
	{
		var tuple = (1, "string", false, -4.2, "foo", 6, 7, 8, 9, 10, 11, 12);
		var expected = $"[1,\"string\",false,{SerializeNumber(-4.2)},\"foo\",6,7,8,9,10,11,12]";
		var actual = JsonSerializer.Serialize(tuple, _options);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TupleInObject()
	{
		var tuple = (false, new ObjectWithTuple { Tuple = (42, "foo") });
		var expected = "[false,{\"Tuple\":[42,\"foo\"]}]";
		var actual = JsonSerializer.Serialize(tuple, _options);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TupleInArray()
	{
		(int, string, bool, double, string, int, int, int)[] tuple = [(1, "string", false, -4.2, "foo", 6, 7, 8), (10, "bool", true, 4.2, "bar", 6, 7, 8)];
		var expected = $"[[1,\"string\",false,{SerializeNumber(-4.2)},\"foo\",6,7,8],[10,\"bool\",true,{SerializeNumber(4.2)},\"bar\",6,7,8]]";
		var actual = JsonSerializer.Serialize(tuple, _options);
		Assert.Equal(expected, actual);
	}

	private static string SerializeNumber(double value)
	{
		return JsonSerializer.Serialize(value, _options);
	}
}
