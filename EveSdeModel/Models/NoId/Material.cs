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
    public class Material : IYamlEntity
    {
        private string _typeID;

        [JsonPropertyName("materialTypeID")]
        public string MaterialTypeID
        {
            get => _typeID;
            set { _typeID = value; }
        }

        [YamlIgnore]
        [JsonPropertyName("typeID")]
        public string TypeID
        {
            get => _typeID;
            set { _typeID = value; }
        }

        [JsonPropertyName("quantity")]
        public string Quantity { get; set; }

        public Material()
        {
            _typeID = string.Empty;
            Quantity = string.Empty;
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            throw new NotImplementedException();
        }

        public void ParseNoId(YamlMappingNode yamlNode) => EveYamlFactory.ParseNoId(this, yamlNode);
    }
}
