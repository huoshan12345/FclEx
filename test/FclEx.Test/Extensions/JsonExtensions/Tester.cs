using Newtonsoft.Json;

namespace FclEx.Test.Extensions.JsonExtensions
{
    internal class Tester
    {
        public string Name { get; set; } = "Name";
        [JsonProperty("Count")] public int Count { get; set; } = 1;
    }
}