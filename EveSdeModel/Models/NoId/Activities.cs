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
    public class Activities : IYamlEntity
    {
        [JsonPropertyName("reaction")]
        public Manufacturing Reaction { get; set; }

        [JsonPropertyName("copying")]
        public BlueprintOperation Copying { get; set; }

        [JsonPropertyName("manufacturing")]
        public Manufacturing Manufacturing { get; set; }

        [JsonPropertyName("invention")]
        public Manufacturing Invention { get; set; }

        [JsonPropertyName("research_material")]
        public BlueprintOperation ResearchMaterial { get; set; }

        [JsonPropertyName("research_time")]
        public BlueprintOperation ResearchTime { get; set; }

        public Activities()
        {
            Reaction = new Manufacturing();
            Copying = new BlueprintOperation();
            Invention = new Manufacturing();
            ResearchMaterial = new BlueprintOperation();
            ResearchTime = new BlueprintOperation();
            Manufacturing = new Manufacturing();
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            throw new NotImplementedException();
        }

        public void ParseNoId(YamlMappingNode yamlNode)
        {
            foreach (var node in yamlNode.Children)
            {
                if (node.Key.ToString() == nameof(Reaction).GetAttr<Activities>())
                {
                    Reaction = EveYamlFactory.GetObject<Manufacturing>((YamlMappingNode)node.Value);
                }
                if (node.Key.ToString() == nameof(Manufacturing).GetAttr<Activities>())
                {
                    Manufacturing = EveYamlFactory.GetObject<Manufacturing>((YamlMappingNode)node.Value);
                }
                if (node.Key.ToString() == nameof(Copying).GetAttr<Activities>())
                {
                    Copying = EveYamlFactory.GetObject<BlueprintOperation>((YamlMappingNode)node.Value);
                }
                if (node.Key.ToString() == nameof(ResearchMaterial).GetAttr<Activities>())
                {
                    ResearchMaterial = EveYamlFactory.GetObject<BlueprintOperation>((YamlMappingNode)node.Value);
                }
                if (node.Key.ToString() == nameof(ResearchTime).GetAttr<Activities>())
                {
                    ResearchTime = EveYamlFactory.GetObject<BlueprintOperation>((YamlMappingNode)node.Value);
                }
            }
        }
    }
}
