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
    /// Уникальная сущность планеты.
    /// </summary>
    public class Planet : IYamlEntity
    {
        public string Id { get; set; }

        [JsonPropertyName("celestialIndex")]
        public string СelestialIndex { get; set; }

        [JsonPropertyName("radius")]
        public string Radius { get; set; }

        [JsonPropertyName("solarSystemID")]
        public string SolarSystemID { get; set; }

        [JsonPropertyName("typeID")]
        public string TypeID { get; set; }

        [JsonPropertyName("moonIDs")]
        public List<string> MoonIDs { get; set; }

        public string Name { get; private set; }

        public Planet()
        {
            Id = string.Empty;
            СelestialIndex = string.Empty;
            Radius = string.Empty;
            SolarSystemID = string.Empty;
            TypeID = string.Empty;
            Name = string.Empty;
            MoonIDs = new List<string>();
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            Id = yamlNode.Key.ToString();
            var properties = EveYamlFactory.GetProperties(GetType());
            foreach (var node in ((YamlMappingNode)yamlNode.Value).Children)
            {
                var found = properties.FirstOrDefault(x => x.Name.GetAttr<Planet>() == node.Key.ToString());
                if (found != null)
                {
                    found.SetValue(this, node.Value.ToString());
                }
                if (node.Key.ToString() == nameof(MoonIDs).GetAttr<Planet>())
                {
                    var mapping = (YamlSequenceNode)node.Value;
                    foreach (YamlScalarNode _node in mapping.Children.OfType<YamlScalarNode>())
                    {
                        if (!string.IsNullOrEmpty(_node.Value))
                            MoonIDs.Add(_node.Value);
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
            return Name;
        }

        public void FillName(IEnumerable<SolarSystem> solarSystems)
        {
            var found = solarSystems.FirstOrDefault(x => x.Id == SolarSystemID);
            if (found != null)
            {
                var parsed = int.Parse(СelestialIndex);
                Name = found.Name.English + " " + RomanConverter.ToRoman(parsed);
            }
        }
    }

    public static class RomanConverter
    {
        private static readonly Dictionary<int, string> RomanMap = new Dictionary<int, string>
        {
            { 100, "C" },
            { 90, "XC" },
            { 50, "L" },
            { 40, "XL" },
            { 10, "X" },
            { 9, "IX" },
            { 5, "V" },
            { 4, "IV" },
            { 1, "I" }
        };

        public static string ToRoman(int number)
        {
            if (number < 1 || number > 100)
                throw new ArgumentOutOfRangeException(nameof(number), "Число должно быть от 1 до 100");

            string result = "";

            foreach (var pair in RomanMap)
            {
                while (number >= pair.Key)
                {
                    result += pair.Value;
                    number -= pair.Key;
                }
            }

            return result;
        }
    }
}
