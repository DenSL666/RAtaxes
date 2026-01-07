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
    public class SolarSystem : IYamlEntity
    {
        public string Id { get; set; }

        [JsonPropertyName("constellationID")]
        public string ConstellationID { get; set; }

        [JsonPropertyName("hub")]
        public string Hub { get; set; }

        [JsonPropertyName("international")]
        public string International { get; set; }

        [JsonPropertyName("radius")]
        public string Radius { get; set; }

        [JsonPropertyName("regionID")]
        public string RegionID { get; set; }

        [JsonPropertyName("regional")]
        public string Regional { get; set; }

        [JsonPropertyName("securityStatus")]
        public string SecurityStatus { get; set; }

        [JsonPropertyName("starID")]
        public string StarID { get; set; }

        [JsonPropertyName("name")]
        public Name Name { get; set; }

        public SolarSystem()
        {
            Id = string.Empty;
            Hub = string.Empty;
            International = string.Empty;
            Radius = string.Empty;
            RegionID = string.Empty;
            Regional = string.Empty;
            SecurityStatus = string.Empty;
            StarID = string.Empty;
            Name = null;
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            Id = yamlNode.Key.ToString();
            var properties = EveYamlFactory.GetProperties(GetType());
            foreach (var node in ((YamlMappingNode)yamlNode.Value).Children)
            {
                var found = properties.FirstOrDefault(x => x.Name.GetAttr<SolarSystem>() == node.Key.ToString());
                if (found != null)
                {
                    found.SetValue(this, node.Value.ToString());
                }
                if (node.Key.ToString() == nameof(Name).GetAttr<SolarSystem>())
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
