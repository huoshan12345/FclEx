namespace FclEx.NewtonsoftJson;

public class ReadSingleOrFirstConverter : ReadSingleConverter
{
    protected override Func<JArray, JToken?> SingleFunc { get; } = array => array.First;
}