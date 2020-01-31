using System.Text.Json.Serialization;

namespace ConvertTalentsToEmporium
{
    public class EmporiumTalent
    {
        [JsonPropertyName("activation")]
        public bool Activation { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("prerequisite")]
        public string Prerequisite { get; set; }

        [JsonPropertyName("ranked")]
        public bool Ranked { get; set; }

        [JsonPropertyName("setting")]
        public string[] Setting { get; set; } = new[] { "Star Wars" };

        [JsonPropertyName("tier")]
        public int Tier { get; set; }

        [JsonPropertyName("turn")]
        public string Turn { get; set; }
    }
}
