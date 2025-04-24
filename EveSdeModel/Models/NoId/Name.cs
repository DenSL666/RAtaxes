using EveSdeModel.Factories;
using EveSdeModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace EveSdeModel.Models
{
    public class Name : IYamlEntity
    {
        [YamlIgnore]
        [JsonPropertyName("de")]
        public string Denmark { get; set; }

        [YamlIgnore]
        [JsonPropertyName("en")]
        public string English { get; set; }

        [YamlIgnore]
        [JsonPropertyName("es")]
        public string Espanol { get; set; }

        [YamlIgnore]
        [JsonPropertyName("fr")]
        public string French { get; set; }

        [YamlIgnore]
        [JsonPropertyName("ja")]
        public string Japan { get; set; }

        [YamlIgnore]
        [JsonPropertyName("ko")]
        public string Korean { get; set; }

        [YamlIgnore]
        [JsonPropertyName("ru")]
        public string Russian { get; set; }

        [YamlIgnore]
        [JsonPropertyName("zh")]
        public string Chineese { get; set; }

        public string en => English;
        public string ru => Russian;

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

        protected Name(Name name)
        {
            Denmark = new string(name.Denmark.ToCharArray());
            English = new string(name.English.ToCharArray());
            Espanol = new string(name.Espanol.ToCharArray());
            French = new string(name.French.ToCharArray());
            Japan = new string(name.Japan.ToCharArray());
            Korean = new string(name.Korean.ToCharArray());
            Russian = new string(name.Russian.ToCharArray());
            Chineese = new string(name.Chineese.ToCharArray());
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            throw new NotImplementedException();
        }

        public void ParseNoId(YamlMappingNode yamlNode) => EveYamlFactory.ParseNoId(this, yamlNode);

        public static Name DeepCopy(Name name)
        {
            return new Name(name);
        }

        public Name DeepCopy()
        {
            return new Name(this);
        }
    }
}
