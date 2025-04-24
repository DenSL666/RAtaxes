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
    public class TypeMaterial : IYamlEntity
    {
        [YamlIgnore]
        private int? _id;

        [YamlIgnore]
        public int TypeId
        {
            get
            {
                if (string.IsNullOrEmpty(Id))
                {
                    return -1;
                }
                else
                {
                    if (!_id.HasValue && int.TryParse(Id, out int _val))
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

        public string Id { get; set; }

        [JsonPropertyName("materials")]
        public List<Material> Materials { get; set; }

        [YamlIgnore]
        public Dictionary<EntityType, string> RefineMaterials { get; }
        [YamlIgnore]
        public EntityType? Entity { get; private set; }

        [YamlIgnore]
        public bool IsAsteroid => Entity != null && Entity.IsPublished && Entity.Group != null && Entity.Group.CategoryID == "25";
        [YamlIgnore]
        public string PortionSize => Entity != null ? Entity.PortionSize : "";

        [YamlIgnore]
        public bool IsIce => IsAsteroid && Entity.Group.Id == "465";
        [YamlIgnore]
        public bool IsUbiquitousMoon4 => IsAsteroid && Entity.Group.Id == "1884";
        [YamlIgnore]
        public bool IsCommonMoon8 => IsAsteroid && Entity.Group.Id == "1920";
        [YamlIgnore]
        public bool IsUncommonMoon16 => IsAsteroid && Entity.Group.Id == "1921";
        [YamlIgnore]
        public bool IsRareMoon32 => IsAsteroid && Entity.Group.Id == "1922";
        [YamlIgnore]
        public bool IsExceptionalMoon64 => IsAsteroid && Entity.Group.Id == "1923";
        [YamlIgnore]
        public bool IsMoon => IsAsteroid && (IsUbiquitousMoon4 || IsCommonMoon8 || IsUncommonMoon16 || IsRareMoon32 || IsExceptionalMoon64);
        [YamlIgnore]
        public bool IsMineral => IsAsteroid && !IsMoon && !IsIce;

        [YamlIgnore]
        private int? _portionSizeInt = null;
        [YamlIgnore]
        public int PortionSizeInt
        {
            get
            {
                if (!_portionSizeInt.HasValue)
                {
                    if (string.IsNullOrEmpty(PortionSize) || !int.TryParse(PortionSize, out int _res))
                        _portionSizeInt = 0;
                    else
                        _portionSizeInt = _res;
                }
                return _portionSizeInt.Value;
            }
        }

        public TypeMaterial()
        {
            Id = string.Empty;
            Materials = new List<Material>();
            Entity = null;

            RefineMaterials = new Dictionary<EntityType, string>();
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            Id = yamlNode.Key.ToString();
            foreach (var node in ((YamlMappingNode)yamlNode.Value).Children)
            {
                if (node.Key.ToString() == nameof(Materials).GetAttr<TypeMaterial>())
                {
                    var mapping = (YamlSequenceNode)node.Value;
                    foreach (YamlMappingNode _node in mapping.Children)
                    {
                        Materials.Add(EveYamlFactory.GetObject<Material>(_node));
                    }
                }
            }
        }

        public void ParseNoId(YamlMappingNode yamlNode)
        {
            throw new NotImplementedException();
        }

        public void FillMaterials(IEnumerable<EntityType> items)
        {
            var foundItem = items.FirstOrDefault(x => x.Id == Id);
            if (foundItem != null)
            {
                Entity = foundItem;
            }

            foreach (var material in Materials)
            {
                var found = items.FirstOrDefault(x => x.Id == material.MaterialTypeID);
                if (found != null)
                {
                    RefineMaterials.Add(found, material.Quantity);
                }
            }
        }

        public Dictionary<EntityType, long> Refine(long count, double efficency)
        {
            var result = new Dictionary<EntityType, long>();
            if (efficency > 0 && PortionSizeInt > 0 && count >= PortionSizeInt)
            {
                long multiple = count / PortionSizeInt;
                //  result = Materials.Sum(x => (int)Math.Floor(Math.Floor(x.Value * multiple * efficiency) * x.Price));
                foreach (var pair in RefineMaterials)
                {
                    if (long.TryParse(pair.Value, out long _value))
                    {
                        var key = pair.Key.DeepCopy();
                        var _res = (long)Math.Floor(_value * multiple * efficency);
                        result.Add(key, _res);
                    }
                }
            }
            return result;
        }

        public Dictionary<EntityType, long> Refine(string count, double efficency)
        {
            if (long.TryParse(count, out long countInt))
                return Refine(countInt, efficency);
            return new Dictionary<EntityType, long>();
        }

        public override string ToString()
        {
            return Entity?.Name?.English ?? "";
        }
    }
}
