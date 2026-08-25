namespace Json.Path.Tests;

//public class ReferenceResolutionTests
//{
//	[Fact]
//	public void ProblemStatementExample()
//	{
//#pragma warning disable 1998
//		var options = new PathEvaluationOptions
//		{
//			ExperimentalFeatures =
//			{
//				ProcessDataReferences = true,
//				DataReferenceDownload = async uri =>
//					uri.OriginalString == "http://example.com/11"
//						? JsonDocument.Parse("{ \"c\": { \"d\": \"Hello\" } }")
//						: null
//			}
//		};
//#pragma warning restore 1998

//		using var instance = JsonDocument.Parse("{ \"a\": { \"b\": {\"$ref\": \"http://example.com/11\" } } }");
//		var path = JsonPath.Parse("$.a.b.c.d");
//		var results = path.Evaluate(instance.RootElement, options);

//		Assert.Equal(1, results.Matches.Count);
//		Assert.Equal("Hello", results.Matches[0].Value.GetString());
//	}

//	[Fact]
//	public void NoResolutionWhen()
//	{
//#pragma warning disable 1998
//		var options = new PathEvaluationOptions
//		{
//			ExperimentalFeatures =
//			{
//				ProcessDataReferences = false,
//				DataReferenceDownload = async uri => throw new Exception("should not be called")
//			}
//		};
//#pragma warning restore 1998

//		using var instance = JsonDocument.Parse("{ \"a\": { \"b\": {\"$ref\": \"http://example.com/11\" } } }");
//		var path = JsonPath.Parse("$.a.b.c.d");
//		var results = path.Evaluate(instance.RootElement, options);

//		Assert.Empty(results.Matches);
//	}
//}
