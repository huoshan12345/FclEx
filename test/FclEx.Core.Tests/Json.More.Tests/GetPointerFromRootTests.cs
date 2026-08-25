using System.Text.Json.Nodes;

namespace Json.More.Tests;

public class GetPointerFromRootTests
{
	[Fact]
	public void BasicPointer()
	{
		var data = new JsonObject
		{
			["foo"] = new JsonArray(0, 1, 2, 3)
		};

		var target = data["foo"]![2]!;

		var expected = "/foo/2";

		var actual = target.GetPointerFromRoot();

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void PointerWithTilde()
	{
		var data = new JsonObject
		{
			["fo~o"] = new JsonArray(0, 1, 2, 3)
		};

		var target = data["fo~o"]![2]!;

		var expected = "/fo~0o/2";

		var actual = target.GetPointerFromRoot();

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void PointerWithSlash()
	{
		var data = new JsonObject
		{
			["fo/o"] = new JsonArray(0, 1, 2, 3)
		};

		var target = data["fo/o"]![2]!;

		var expected = "/fo~1o/2";

		var actual = target.GetPointerFromRoot();

		Assert.Equal(expected, actual);
	}
}
