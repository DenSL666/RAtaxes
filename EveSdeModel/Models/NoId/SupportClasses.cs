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
    public class BaseProduct
    {
        [YamlIgnore]
        private int? _id;

        /// <summary>
        /// Числовый вид Id сущности.
        /// </summary>
        [YamlIgnore]
        public int IntId
        {
            get
            {
                if (string.IsNullOrEmpty(TypeID))
                {
                    return -1;
                }
                else
                {
                    if (!_id.HasValue && int.TryParse(TypeID, out int _val))
                    {
                        _id = _val;
                    }
                    if (_id.HasValue)
                        return _id.Value;
                    else
                        return -1;
                }
            }
        }

        [JsonPropertyName("quantity")]
        public string Quantity { get; set; }

        [JsonPropertyName("probability")]
        public string Probability { get; set; }

        [JsonPropertyName("typeID")]
        public string TypeID { get; set; }

        public BaseProduct()
        {
            Quantity = string.Empty;
            Probability = string.Empty;
            TypeID = string.Empty;
        }
    }

    public class Product : BaseProduct, IYamlEntity
    {
        public Product() : base() { }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            throw new NotImplementedException();
        }

        public void ParseNoId(YamlMappingNode yamlNode) => EveYamlFactory.ParseNoId(this, yamlNode);
    }

    public class Skill : IYamlEntity
    {
        [JsonPropertyName("level")]
        public string Level { get; set; }

        [JsonPropertyName("typeID")]
        public string TypeID { get; set; }

        public Skill()
        {
            Level = string.Empty;
            TypeID = string.Empty;
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            throw new NotImplementedException();
        }

        public void ParseNoId(YamlMappingNode yamlNode) => EveYamlFactory.ParseNoId(this, yamlNode);
    }
}
