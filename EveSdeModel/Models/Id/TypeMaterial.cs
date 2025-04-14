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
    public class TypeMaterial : IYamlEntity
    {
        public string Id { get; set; }

        [JsonPropertyName("materials")]
        public List<Material> Materials { get; set; }

        public TypeMaterial()
        {
            Id = string.Empty;
            Materials = new List<Material>();
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            Id = yamlNode.Key.ToString();
            foreach (var node in ((YamlMappingNode)yamlNode.Value).Children)
            {
                if (node.Key.ToString() == nameof(Materials).GetAttr<TypeMaterial>())
                {
                    var mapping = (YamlSequenceNode)node.Value;
                    foreach (YamlMappingNode _node in mapping.Children)
                    {
                        Materials.Add(EveYamlFactory.GetObject<Material>(_node));
                    }
                }
            }
        }

        public void ParseNoId(YamlMappingNode yamlNode)
        {
            throw new NotImplementedException();
        }
    }
}
