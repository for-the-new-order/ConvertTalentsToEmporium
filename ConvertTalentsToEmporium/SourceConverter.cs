using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace ConvertTalentsToEmporium
{
    public class SourceConverter
    {
        private readonly Deserializer _yamlDeserializer;
        private readonly JsonSerializerOptions _jsonOptions;

        public SourceConverter(JsonSerializerOptions jsonOptions, Deserializer yamlDeserializer)
        {
            _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
            _yamlDeserializer = yamlDeserializer ?? throw new ArgumentNullException(nameof(yamlDeserializer));
        }

        public async Task ConvertAsync(string sourcePath, EmporiumBuilder builder)
        {
            var isSourceYaml = Path.GetExtension(sourcePath) == ".yml";
            using var source = File.OpenRead(sourcePath);
            var talents = await Deserialize(source, isSourceYaml);
            foreach (var talent in talents)
            {
                if (string.IsNullOrEmpty(talent.Depreciated))
                {
                    builder.Add(talent);
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
}
