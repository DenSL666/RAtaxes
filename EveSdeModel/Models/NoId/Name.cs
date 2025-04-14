using EveSdeModel.Interfaces;
using EveSdeModel.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YamlDotNet.RepresentationModel;
using System.Text.Json.Serialization;

namespace EveSdeModel.Models
{
    public class Name : IYamlEntity
    {
        [JsonPropertyName("de")]
        public string Denmark { get; set; }

        [JsonPropertyName("en")]
        public string English { get; set; }

        [JsonPropertyName("es")]
        public string Espanol { get; set; }

        [JsonPropertyName("fr")]
        public string French { get; set; }

        [JsonPropertyName("ja")]
        public string Japan { get; set; }

        [JsonPropertyName("ko")]
        public string Korean { get; set; }

        [JsonPropertyName("ru")]
        public string Russian { get; set; }

        [JsonPropertyName("zh")]
        public string Chineese { get; set; }

        public Name()
        {
            Denmark = string.Empty;
            English = string.Empty;
            Espanol = string.Empty;
            French = string.Empty;
            Japan = string.Empty;
            Korean = string.Empty;
            Russian = string.Empty;
            Chineese = string.Empty;
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            throw new NotImplementedException();
        }

        public void ParseNoId(YamlMappingNode yamlNode) => EveYamlFactory.ParseNoId(this, yamlNode);
    }
}
