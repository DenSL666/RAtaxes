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
    /// Уникальная сущность луны.
    /// </summary>
    public class Moon : IYamlEntity
    {
        public string Id { get; set; }

        [JsonPropertyName("celestialIndex")]
        public string СelestialIndex { get; set; }

        [JsonPropertyName("orbitIndex")]
        public string OrbitIndex { get; set; }

        [JsonPropertyName("orbitID")]
        public string OrbitID { get; set; }

        [JsonPropertyName("solarSystemID")]
        public string SolarSystemID { get; set; }

        [JsonPropertyName("typeID")]
        public string TypeID { get; set; }

        public string Name { get; private set; }

        public Moon()
        {
            Id = string.Empty;
            СelestialIndex = string.Empty;
            OrbitID = string.Empty;
            SolarSystemID = string.Empty;
            TypeID = string.Empty;
            Name = string.Empty;
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            Id = yamlNode.Key.ToString();
            var properties = EveYamlFactory.GetProperties(GetType());
            foreach (var node in ((YamlMappingNode)yamlNode.Value).Children)
            {
                var found = properties.FirstOrDefault(x => x.Name.GetAttr<Moon>() == node.Key.ToString());
                if (found != null)
                {
                    found.SetValue(this, node.Value.ToString());
                }
            }
        }

        public void ParseNoId(YamlMappingNode yamlNode)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Name;
        }

        public void FillName(IEnumerable<Planet> planets)
        {
            var found = planets.FirstOrDefault(x => x.Id == OrbitID);
            if (found != null)
            {
                Name = found.Name + " - " + OrbitIndex;
            }
        }
    }
}
