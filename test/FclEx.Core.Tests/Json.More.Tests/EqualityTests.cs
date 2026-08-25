using System.Text.Json.Nodes;

namespace Json.More.Tests;

public class EqualityTests
{
	[Fact]
	public void NumbersAreEqualEvenIfOneHasTrailingZero()
	{
		JsonNode intFour = 4;
		JsonNode floatFour = 4.0;

		Assert.True(intFour.IsEquivalentTo(floatFour));
	}

	[Fact]
	public void NumbersAreEqualEvenIfOneHasTrailingZero_Decimal()
	{
		JsonNode intFour = (decimal) 4;
		JsonNode floatFour = (decimal) 4.0;

		Assert.True(intFour.IsEquivalentTo(floatFour));
	}

	[Fact]
	public void NumbersAreEqualEvenIfOneHasTrailingZero_IntAndDecimal()
	{
		JsonNode intFour = 4;
		JsonNode floatFour = 4.0m;

		Assert.True(intFour.IsEquivalentTo(floatFour));
	}

	[Fact]
	public void NumbersAreEqualEvenIfOneHasTrailingZero_Summed()
	{
		decimal a = 4;
		decimal b = (decimal)4.00;
		JsonNode explicitFour = a;
		JsonNode summedFour = b;

		Assert.True(explicitFour.IsEquivalentTo(summedFour));
	}

	[Fact]
	public void NumbersAreEqualEvenIfOneHasTrailingZero_Parsed()
	{
		var intFour = JsonNode.Parse("4");
		var floatFour = JsonNode.Parse("4.0");

		Assert.True(intFour.IsEquivalentTo(floatFour));
	}
}
