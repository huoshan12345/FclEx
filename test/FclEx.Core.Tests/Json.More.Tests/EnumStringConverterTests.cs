using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.More.Tests;

public class EnumStringConverterTests
{
	private class ConversionTest
	{
		[JsonConverter(typeof(EnumStringConverter<DayOfWeek>))]
		public DayOfWeek Day { get; set; }
	}

	[Fact]
	public void DayOfWeekAsPropertyIsConverted()
	{
		var expected = "{\"Day\":\"Wednesday\"}";
		var actual = JsonSerializer.Serialize(new ConversionTest { Day = DayOfWeek.Wednesday });

		Assert.Equal(expected, actual);

		var deserialized = JsonSerializer.Deserialize<ConversionTest>(actual)!;

		Assert.Equal(DayOfWeek.Wednesday, deserialized.Day);
	}

	[JsonConverter(typeof(EnumStringConverter<CustomEnum>))]
	public enum CustomEnum
	{
		Zero,
		[System.ComponentModel.Description("one")]
		One,
		Two
	}

	[Theory]
	[InlineData(CustomEnum.Zero, "Zero")]
	[InlineData(CustomEnum.One, "one")]
	[InlineData(CustomEnum.Two, "Two")]
	public void CustomEnumIsConverted(CustomEnum value, string serializedValue)
	{
		var expected = $"\"{serializedValue}\"";
		var actual = JsonSerializer.Serialize(value);

		Assert.Equal(expected, actual);

		var deserialized = JsonSerializer.Deserialize<CustomEnum>(actual);

		Assert.Equal(value, deserialized);
	}

	[JsonConverter(typeof(EnumStringConverter<CustomFlagsEnum>))]
	[Flags]
	private enum CustomFlagsEnum
	{
		Zero,
		One = 1,
		Two = 2
	}

	private class FlagsEnumContainer
	{
		public CustomFlagsEnum Value { get; set; }
	}

	[Fact]
	public void CustomFlagsEnumIsConverted()
	{
		var value = new FlagsEnumContainer { Value = CustomFlagsEnum.One | CustomFlagsEnum.Two };

		var expected = "{\"Value\":[\"One\",\"Two\"]}";
		var actual = JsonSerializer.Serialize(value);

		Assert.Equal(expected, actual);

		var deserialized = JsonSerializer.Deserialize<FlagsEnumContainer>(actual)!;

		Assert.Equal(CustomFlagsEnum.One | CustomFlagsEnum.Two, deserialized.Value);
	}

	private class EnumWithDuplicatedMemberValuesContainer
	{
		[JsonConverter(typeof(EnumStringConverter<HttpStatusCode>))]
		public HttpStatusCode Value { get; set; }
	}

	[Fact]
	public void EnumWithDuplicatedMemberValuesIsConverted()
	{
		var value = new EnumWithDuplicatedMemberValuesContainer { Value = HttpStatusCode.MultipleChoices };

		var expected = "{\"Value\":\"MultipleChoices\"}";
		var actual = JsonSerializer.Serialize(value);

		Assert.Equal(expected, actual);

		var deserialized = JsonSerializer.Deserialize<EnumWithDuplicatedMemberValuesContainer>(actual)!;

		Assert.Equal(HttpStatusCode.MultipleChoices, deserialized.Value);
	}
}
