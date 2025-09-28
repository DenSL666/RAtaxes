using EveSdeModel.Factories;
using EveSdeModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace EveSdeModel.Models
{
    /// <summary>
    /// Контейнер активнсотей, которые можно проводить с чертежом.
    /// </summary>
    public class Activities : IYamlEntity
    {
        /// <summary>
        /// Создание продуктов формулы реакции.
        /// </summary>
        [JsonPropertyName("reaction")]
        public Manufacturing Reaction { get; set; }

        /// <summary>
        /// Создание копии из оригинала.
        /// </summary>
        [JsonPropertyName("copying")]
        public BlueprintOperation Copying { get; set; }

        /// <summary>
        /// Производство продукта.
        /// </summary>
        [JsonPropertyName("manufacturing")]
        public Manufacturing Manufacturing { get; set; }

        /// <summary>
        /// Превращение т1 копии в т2 копию.
        /// </summary>
        [JsonPropertyName("invention")]
        public Manufacturing Invention { get; set; }

        /// <summary>
        /// Улучшение оригинала по эффективности расхода материалов.
        /// </summary>
        [JsonPropertyName("research_material")]
        public BlueprintOperation ResearchMaterial { get; set; }

        /// <summary>
        /// Улучшение оригинала по эффективности расхода времени.
        /// </summary>
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
