namespace FclEx.NewtonsoftJson;

public class ReadSingleOrLastConverter : ReadSingleConverter
{
    protected override Func<JArray, JToken?> SingleFunc { get; } = array => array.Last;
}