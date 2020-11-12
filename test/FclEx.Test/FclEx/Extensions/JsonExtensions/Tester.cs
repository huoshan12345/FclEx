using Newtonsoft.Json;

namespace FclEx.Extensions.JsonExtensions
{
    internal class Tester
    {
        public string Name { get; set; } = "Name";
        [JsonProperty("Count")] public int Count { get; set; } = 1;
    }
}