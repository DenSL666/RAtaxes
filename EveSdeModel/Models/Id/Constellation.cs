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
    /// Уникальная сущность созвездия.
    /// </summary>
    public class Constellation : IYamlEntity
    {
        public string Id { get; set; }

        [JsonPropertyName("factionID")]
        public string FactionID { get; set; }

        [JsonPropertyName("wormholeClassID")]
        public string WormholeClassID { get; set; }

        [JsonPropertyName("regionID")]
        public string RegionID { get; set; }

        [JsonPropertyName("solarSystemIDs")]
        public List<string> SolarSystemIDs { get; set; }

        [JsonPropertyName("name")]
        public Name Name { get; set; }

        public Constellation()
        {
            Id = string.Empty;
            FactionID = string.Empty;
            WormholeClassID = string.Empty;
            RegionID = string.Empty;
            Name = null;
            SolarSystemIDs = new List<string>();
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            Id = yamlNode.Key.ToString();
            var properties = EveYamlFactory.GetProperties(GetType());
            foreach (var node in ((YamlMappingNode)yamlNode.Value).Children)
            {
                var found = properties.FirstOrDefault(x => x.Name.GetAttr<Constellation>() == node.Key.ToString());
                if (found != null)
                {
                    found.SetValue(this, node.Value.ToString());
                }
                if (node.Key.ToString() == nameof(Name).GetAttr<Constellation>())
                {
                    Name = EveYamlFactory.GetObject<Name>((YamlMappingNode)node.Value);
                }
                if (node.Key.ToString() == nameof(SolarSystemIDs).GetAttr<Constellation>())
                {
                    var mapping = (YamlSequenceNode)node.Value;
                    foreach (YamlScalarNode _node in mapping.Children.OfType<YamlScalarNode>())
                    {
                        if (!string.IsNullOrEmpty(_node.Value))
                            SolarSystemIDs.Add(_node.Value);
                    }
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
