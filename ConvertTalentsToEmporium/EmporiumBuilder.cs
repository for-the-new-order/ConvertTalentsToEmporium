using System;
using System.Text;

namespace ConvertTalentsToEmporium
{
    public class EmporiumBuilder
    {
        private readonly StringBuilder _json;
        private readonly EmporiumTalentBuilder _emporiumTalentBuilder;

        public EmporiumBuilder(StringBuilder json, EmporiumTalentBuilder emporiumTalentBuilder)
        {
            _json = json ?? throw new ArgumentNullException(nameof(json));
            _emporiumTalentBuilder = emporiumTalentBuilder ?? throw new ArgumentNullException(nameof(emporiumTalentBuilder));
        }

        public void Begin()
        {
            _json.Clear();
            _json.Append(@"[{""customData"": {""customTalents"": {");
        }

        public void Add(WebTalent talent)
        {
            _emporiumTalentBuilder.Add(talent);
        }

        public void End()
        {
            _json.Append(@"}}}]");
        }

        public string ToJson()
        {
            return _json.ToString();
        }
    }
}
