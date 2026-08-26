namespace Json.Path.Tests;

public class DeferredExecutionTests
{
	[Fact]
	public void RepeatedQueryOnChangedDataSet()
	{
		var data = new JsonArray("bob", "sam", "alice");
		var query = JsonPath.Parse("$[? length(@) > 3 ]");

		var result = query.Evaluate(data);

		Assert.Equal(1, result.Matches!.Count);

		data[0] = "sally";

		Assert.Equal(2, result.Matches!.Count);
	}
}
