using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace ConvertTalentsToEmporium
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting...");
            var path = FindPath(args);
            var jsonOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                WriteIndented = false
            };
            var yamlDeserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
            var talentConverter = new SourceConverter(jsonOptions, yamlDeserializer);

            var emporiumBuilder = new EmporiumBuilder();
            using (var talentBuilder = emporiumBuilder.Create(jsonOptions))
            {
                foreach (var source in path.sourcePaths)
                {
                    await talentConverter.ConvertAsync(source, talentBuilder);
                }
            }

            var destinationWriter = new DestinationWriter();
            await destinationWriter.SaveAsync(path.destinationPath, emporiumBuilder);
            Console.WriteLine("Done!");
        }

        private static (string[] sourcePaths, string destinationPath) FindPath(string[] args)
        {
            if (args.Length >= 2)
            {
                var sources = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++)
                {
                    sources[i] = EnforceValidPath(args[i], PathType.Source);
                }
                var dest = EnforceValidPath(args[args.Length - 1], PathType.Destination);
                return (sources, dest);
            }
            else
            {
                var source = EnforceValidPath(null, PathType.Source);
                var dest = EnforceValidPath(null, PathType.Destination);
                return (new[] { source }, dest);
            }
        }

        private static string EnforceValidPath(string path, PathType pathType)
        {
            if (path == null)
            {
                Console.WriteLine($"Please enter path for '{pathType}': ");
                path = Console.ReadLine();
            }
            if (pathType == PathType.Source && !File.Exists(path))
            {
                Console.WriteLine($"Path '{path}' does not exist.");
                path = EnforceValidPath(null, pathType);
            }
            else if (pathType == PathType.Destination)
            {
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Console.WriteLine($"Directory '{directory}' does not exist.");
                    path = EnforceValidPath(null, pathType);
                }
            }
            return path;
        }

        public enum PathType
        {
            Source, 
            Destination
        }
    }

    public class DestinationWriter
    {
        public async Task SaveAsync(string destinationPath, EmporiumBuilder emporiumBuilder)
        {
            var json = emporiumBuilder.ToString();
            await File.WriteAllTextAsync(destinationPath, json);
        }
    }

    public class SourceConverter
    {
        private readonly Deserializer _yamlDeserializer;
        private readonly JsonSerializerOptions _jsonOptions;

        public SourceConverter(JsonSerializerOptions jsonOptions, Deserializer yamlDeserializer)
        {
            _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
            _yamlDeserializer = yamlDeserializer ?? throw new ArgumentNullException(nameof(yamlDeserializer));
        }

        public async Task ConvertAsync(string sourcePath, EmporiumTalentBuilder talentBuilder)
        {
            var isSourceYaml = Path.GetExtension(sourcePath) == ".yml";
            using var source = File.OpenRead(sourcePath);
            var talents = await Deserialize(source, isSourceYaml);
            foreach (var talent in talents)
            {
                if (string.IsNullOrEmpty(talent.Depreciated))
                {
                    talentBuilder.Add(talent);
                }
            }
        }

        private async Task<WebTalent[]> Deserialize(FileStream source, bool isYaml)
        {
            if (isYaml)
            {
                using var streamReader = new StreamReader(source);
                return _yamlDeserializer.Deserialize<WebTalent[]>(streamReader);
            }
            else
            {
                return await JsonSerializer.DeserializeAsync<WebTalent[]>(source, _jsonOptions);
            }
        }
    }

    public class EmporiumBuilder
    {
        private readonly StringBuilder _json = new StringBuilder();

        public EmporiumTalentBuilder Create(JsonSerializerOptions jsonOptions)
        {
            _json.Append(@"[{""customData"": {""customTalents"": {");
            return new EmporiumTalentBuilder(_json, jsonOptions);
        }

        public override string ToString()
        {
            return _json.ToString();
        }

    }
    public class EmporiumTalentBuilder : IDisposable
    {
        private readonly StringBuilder _json;
        private readonly JsonSerializerOptions _jsonOptions;

        public EmporiumTalentBuilder(StringBuilder json, JsonSerializerOptions jsonOptions)
        {
            _json = json ?? throw new ArgumentNullException(nameof(json));
            _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
        }

        private int talentCount = 0;
        public void Add(WebTalent talent)
        {
            if (talentCount++ > 0) { _json.Append(","); }


            var formattedName = FormatName(talent.Name);
            _json.Append($"\"{formattedName}\":");

            var emporiumTalent = Map(talent);
            var jsonObject = JsonSerializer.Serialize(emporiumTalent, _jsonOptions);
            _json.Append(jsonObject);
        }

        private EmporiumTalent Map(WebTalent talent)
        {
            var result = new EmporiumTalent
            {
                Activation = talent.Activation != "Passive",
                Description = talent.Text,
                Name= talent.Name,
                Ranked= talent.Ranked == "Yes",
                Tier = talent.Tier,
                Turn = ParseTurn(talent.Activation)
            };

            var isRealmsOfTerrinoth = (talent.Source?.Contains("ROT")).GetValueOrDefault();
            if (isRealmsOfTerrinoth)
            {
                result.Setting = new[] { "Fantasy" };
            }

            var isShadowOfTheBeanstalk = (talent.Source?.Contains("SOTB")).GetValueOrDefault();
            if (isShadowOfTheBeanstalk)
            {
                result.Setting = new[] { "Steampunk" };
            }

            return result;
        }

        private readonly Regex _parseTurnRegex = new Regex("Active \\((?<turn>[^\\)]+)\\)");
        private string ParseTurn(string activation)
        {
            if (_parseTurnRegex.IsMatch(activation))
            {
                return _parseTurnRegex.Match(activation).Groups["turn"].Value;
            }
            return null;
        }

        private readonly Regex _nameRegex = new Regex("[^a-zA-Z]");
        private const int MaxNameLength = 24;
        private string FormatName(string name)
        {
            var sanitizedName = _nameRegex.Replace(name, "");
            if (sanitizedName.Length > MaxNameLength)
            {
                return sanitizedName.Substring(0, MaxNameLength);
            }
            return sanitizedName;
        }

        public void Dispose()
        {
            _json.Append(@"}}}]");
        }
    }

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
