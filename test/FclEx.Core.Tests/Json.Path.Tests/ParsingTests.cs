using System.Collections.Generic;

namespace Json.Path.Tests;

public class ParsingTests
{
	public static IEnumerable<object[]> SuccessCases =>
		[
			["$['foo']"],
			["$[ 'foo']"],
			["$['foo' ]"],

			["$[1]"],
			["$[ 1]"],
			["$[1 ]"],
			["$[42]"],
			["$[-1]"],
			["$[-42]"],

			["$[*]"],
			["$[ *]"],
			["$[* ]"],

			["$[:]"],
			["$[1:]"],
			["$[1::]"],
			["$[1:2]"],
			["$[1:2:]"],
			["$[1::3]"],
			["$[1:2:3]"],
			["$[-1:2:3]"],
			["$[1:-2:3]"],
			["$[1:2:-3]"],
			["$[ 1:2:3]"],
			["$[1 :2:3]"],
			["$[1: 2:3]"],
			["$[1:2 :3]"],
			["$[1:2: 3]"],
			["$[1:2:3 ]"],

			["$.foo"],

			["$.*"],

			["$..foo"],
			["$..*"],
			["$..[1]"],
			["$..[1,2]"],

			["$[?@.foo]"],
			["$[?(@.foo)]"],
			["$[?(@.foo && @.bar)]"],
			["$[?(@.foo && @.bar || @.baz)]"],
			["$[?(@.foo || @.bar && @.baz)]"],
			["$[?(!@.foo)]"],
			["$[?(@.foo && !@.bar)]"],
			["$[?!(@.foo == false)]"],
			["$[?(@.foo == false)]"],
			["$[?(@['name'] == null || @['name'] == 'abc')]"],

			["$[1,'foo',1:2:3,*]"],
		];

	[Theory]
	[MemberData(nameof(SuccessCases))]
	public void ParseSuccess(string path)
	{
		Console.WriteLine(JsonPath.Parse(path));
	}

	public static IEnumerable<object[]> OptionalMathCases =>
		[
			["$[?(@.foo==(4+5))]"],
			["$[?(@.foo==2*(4+5))]"],
			["$[?(@.foo==2+(4+5))]"],
			["$[?(@.foo==2-(4+5))]"],
			["$[?(@.foo==2*4+5)]"],
			["$[?((4+5)==@.foo)]"],
			["$[?(2*(4+5)==@.foo)]"],
			["$[?(2+(4+5)==@.foo)]"],
			["$[?(2-(4+5)==@.foo)]"],
			["$[?(2*4+5==@.foo)]"],
			["$[?@.foo==(4+5)]"],
			["$[?@.foo==2*(4+5)]"],
			["$[?@.foo==2+(4+5)]"],
			["$[?@.foo==2-(4+5)]"],
			["$[?@.foo==2*4+5]"],
			["$[?(4+5)==@.foo]"],
			["$[?2*(4+5)==@.foo]"],
			["$[?2+(4+5)==@.foo]"],
			["$[?2-(4+5)==@.foo]"],
			["$[?2*4+5==@.foo]"],
		];

	[Theory]
	[MemberData(nameof(OptionalMathCases))]
	public void ParseMathWithOptions(string path)
	{
		Console.WriteLine(JsonPath.Parse(path, new PathParsingOptions{AllowMathOperations = true}));
	}

	[Theory]
	[MemberData(nameof(OptionalMathCases))]
	public void ParseMathWithoutOptions(string path)
	{
		Assert.Throws<PathParseException>(() => JsonPath.Parse(path));
	}

	public static IEnumerable<object[]> OptionalJsonLiteralCases =>
		[
			["$[?@.foo==[1,2,3]]"],
			["$[?@.foo=={\"bar\":\"object\"}]"],
		];

	[Theory]
	[MemberData(nameof(OptionalJsonLiteralCases))]
	public void ParseLiteralWithOptions(string path)
	{
		Console.WriteLine(JsonPath.Parse(path, new PathParsingOptions{AllowJsonConstructs = true}));
	}

	[Theory]
	[MemberData(nameof(OptionalJsonLiteralCases))]
	public void ParseLiteralWithoutOptions(string path)
	{
		Assert.Throws<PathParseException>(() => JsonPath.Parse(path));
	}

	public static IEnumerable<object[]> OptionalInOpCases =>
		[
			["$[?5 in @.foo]"],
		];

	[Theory]
	[MemberData(nameof(OptionalInOpCases))]
	public void ParseInOpWithOptions(string path)
	{
		Console.WriteLine(JsonPath.Parse(path, new PathParsingOptions{AllowInOperator = true}));
	}

	[Theory]
	[MemberData(nameof(OptionalInOpCases))]
	public void ParseInOpWithoutOptions(string path)
	{
		Assert.Throws<PathParseException>(() => JsonPath.Parse(path));
	}

	[Theory]
	[MemberData(nameof(SuccessCases))]
	public void ParseRelativeStarts(string path)
	{
		path = $"@{path[1..]}"; // Turn the absolute path into a relative path
		JsonPath.Parse(path, new PathParsingOptions(){AllowRelativePathStart = true});
	}

	[Theory]
	[MemberData(nameof(SuccessCases))]
	public void TryParseRelativeStarts(string path)
	{
		path = $"@{path[1..]}"; // Turn the absolute path into a relative path
		Assert.True(JsonPath.TryParse(path, out _, new PathParsingOptions(){AllowRelativePathStart = true}));
	}
}
