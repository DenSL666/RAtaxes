using EveSdeModel.Factories;
using EveSdeModel.Interfaces;
using EveSdeModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace EveSdeModel.Models
{
    public class Manufacturing : IYamlEntity
    {
        [JsonPropertyName("materials")]
        public List<Material> Materials { get; set; }

        [JsonPropertyName("products")]
        public List<Product> Products { get; set; }

        [JsonPropertyName("skills")]
        public List<Skill> Skills { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        public Manufacturing()
        {
            Materials = new List<Material>();
            Products = new List<Product>();
            Skills = new List<Skill>();
            Time = string.Empty;
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            throw new NotImplementedException();
        }

        public void ParseNoId(YamlMappingNode yamlNode)
        {
            var properties = EveYamlFactory.GetProperties(GetType());
            foreach (var node in yamlNode.Children)
            {
                var found = properties.FirstOrDefault(x => x.Name.GetAttr<Manufacturing>() == node.Key.ToString());
                if (found != null)
                {
                    found.SetValue(this, node.Value.ToString());
                }

                if (node.Key.ToString() == nameof(Materials).GetAttr<Manufacturing>())
                {
                    var mapping = (YamlSequenceNode)node.Value;
                    foreach (YamlMappingNode _node in mapping.Children)
                    {
                        Materials.Add(EveYamlFactory.GetObject<Material>(_node));
                    }
                }

                if (node.Key.ToString() == nameof(Products).GetAttr<Manufacturing>())
                {
                    var mapping = (YamlSequenceNode)node.Value;
                    foreach (YamlMappingNode _node in mapping.Children)
                    {
                        Products.Add(EveYamlFactory.GetObject<Product>(_node));
                    }
                }

                if (node.Key.ToString() == nameof(Skills).GetAttr<Manufacturing>())
                {
                    var mapping = (YamlSequenceNode)node.Value;
                    foreach (YamlMappingNode _node in mapping.Children)
                    {
                        Skills.Add(EveYamlFactory.GetObject<Skill>(_node));
                    }
                }
            }
        }
    }
}
