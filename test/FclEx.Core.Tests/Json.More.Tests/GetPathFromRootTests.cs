using System.Text.Json.Nodes;

namespace Json.More.Tests;

public class GetPathFromRootTests
{
	[Fact]
	public void BasicPath()
	{
		var data = new JsonObject
		{
			["foo"] = new JsonArray(0, 1, 2, 3)
		};

		var target = data["foo"]![2]!;

		var expected = "$['foo'][2]";

		var actual = target.GetPathFromRoot();

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void BasicPathGetShorthand()
	{
		var data = new JsonObject
		{
			["foo"] = new JsonArray(0, 1, 2, 3)
		};

		var target = data["foo"]![2]!;

		var expected = "$.foo[2]";

		var actual = target.GetPathFromRoot(true);

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void PathWithSingleQuoteInKey()
	{
		var data = new JsonObject
		{
			["fo'o"] = new JsonArray(0, 1, 2, 3)
		};

		var target = data["fo'o"]![2]!;

		var expected = "$['fo\\'o'][2]";

		var actual = target.GetPathFromRoot();

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void PathWithDoubleQuoteInKey()
	{
		var data = new JsonObject
		{
			["fo\"o"] = new JsonArray(0, 1, 2, 3)
		};

		var target = data["fo\"o"]![2]!;

		var expected = "$['fo\"o'][2]";

		var actual = target.GetPathFromRoot();

		Assert.Equal(expected, actual);
	}
}
