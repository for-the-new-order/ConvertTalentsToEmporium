using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ConvertTalentsToEmporium
{
    public class EmporiumTalentBuilder
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
    }
}
