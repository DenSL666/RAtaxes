using EveSdeModel.Factories;
using EveSdeModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace EveSdeModel.Models.Id
{
    /// <summary>
    /// Уникальная сущность региона.
    /// </summary>
    public class Region : IYamlEntity
    {
        public string Id { get; set; }

        [JsonPropertyName("factionID")]
        public string FactionID { get; set; }

        [JsonPropertyName("wormholeClassID")]
        public string WormholeClassID { get; set; }

        [JsonPropertyName("name")]
        public Name Name { get; set; }

        public Region()
        {
            Id = string.Empty;
            FactionID = string.Empty;
            WormholeClassID = string.Empty;
            Name = null;
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            Id = yamlNode.Key.ToString();
            var properties = EveYamlFactory.GetProperties(GetType());
            foreach (var node in ((YamlMappingNode)yamlNode.Value).Children)
            {
                var found = properties.FirstOrDefault(x => x.Name.GetAttr<Region>() == node.Key.ToString());
                if (found != null)
                {
                    found.SetValue(this, node.Value.ToString());
                }
                if (node.Key.ToString() == nameof(Name).GetAttr<Region>())
                {
                    Name = EveYamlFactory.GetObject<Name>((YamlMappingNode)node.Value);
                }
            }
        }

        public void ParseNoId(YamlMappingNode yamlNode)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Name?.en ?? "";
        }
    }
}
