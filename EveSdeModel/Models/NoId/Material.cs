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
    public class BaseMaterial
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
                if (string.IsNullOrEmpty(_typeID))
                {
                    return -1;
                }
                else
                {
                    if (!_id.HasValue && int.TryParse(_typeID, out int _val))
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

        private string _typeID;

        [JsonPropertyName("materialTypeID")]
        public string MaterialTypeID
        {
            get => _typeID;
            set { _typeID = value; }
        }

        [YamlIgnore]
        [JsonPropertyName("typeID")]
        public string TypeID
        {
            get => _typeID;
            set { _typeID = value; }
        }

        [JsonPropertyName("quantity")]
        public string Quantity { get; set; }

        public BaseMaterial()
        {
            _typeID = string.Empty;
            Quantity = string.Empty;
        }
    }

    public class Material : BaseMaterial, IYamlEntity
    {
        public Material() : base() { }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            throw new NotImplementedException();
        }

        public void ParseNoId(YamlMappingNode yamlNode) => EveYamlFactory.ParseNoId(this, yamlNode);
    }
}
