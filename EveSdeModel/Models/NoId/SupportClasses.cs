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
    public class Product : IYamlEntity
    {
        [JsonPropertyName("quantity")]
        public string Quantity { get; set; }

        [JsonPropertyName("probability")]
        public string Probability { get; set; }

        [JsonPropertyName("typeID")]
        public string TypeID { get; set; }

        public Product()
        {
            Quantity = string.Empty;
            Probability = string.Empty;
            TypeID = string.Empty;
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            throw new NotImplementedException();
        }

        public void ParseNoId(YamlMappingNode yamlNode) => EveYamlFactory.ParseNoId(this, yamlNode);
    }

    public class Skill : IYamlEntity
    {
        [JsonPropertyName("level")]
        public string Level { get; set; }

        [JsonPropertyName("typeID")]
        public string TypeID { get; set; }

        public Skill()
        {
            Level = string.Empty;
            TypeID = string.Empty;
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            throw new NotImplementedException();
        }

        public void ParseNoId(YamlMappingNode yamlNode) => EveYamlFactory.ParseNoId(this, yamlNode);
    }
}
