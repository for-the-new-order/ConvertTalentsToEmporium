using System.Text.Json.Serialization;

namespace ConvertTalentsToEmporium
{
    public class WebTalent
    {
        [JsonPropertyName(nameof(Name))]
        public string Name { get; set; }

        [JsonPropertyName(nameof(Tier))]
        public int Tier { get; set; }

        [JsonPropertyName(nameof(Activation))]
        public string Activation { get; set; }

        [JsonPropertyName(nameof(Ranked))]
        public string Ranked { get; set; }

        [JsonPropertyName(nameof(Text))]
        public string Text { get; set; }

        [JsonPropertyName(nameof(Source))]
        public string Source { get; set; }

        [JsonPropertyName(nameof(From))]
        public string From { get; set; }

        [JsonPropertyName(nameof(Depreciated))]
        public string Depreciated { get; set; }
    }
}
