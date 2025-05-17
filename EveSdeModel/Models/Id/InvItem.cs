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
    public class InvItem : IYamlEntity
    {
        [JsonPropertyName("itemID")]
        public string Id { get; set; }

        [JsonPropertyName("flagID")]
        public string FlagID { get; set; }

        [JsonPropertyName("locationID")]
        public string LocationID { get; set; }

        [JsonPropertyName("ownerID")]
        public string OwnerID { get; set; }

        [JsonPropertyName("quantity")]
        public string Quantity { get; set; }

        [JsonPropertyName("typeID")]
        public string TypeID { get; set; }

        public string Name { get; set; } 

        public InvItem()
        {
            Id = string.Empty;
            FlagID = string.Empty;
            LocationID = string.Empty;
            OwnerID = string.Empty;
            Quantity = string.Empty;
            TypeID = string.Empty;
            Name = string.Empty;
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            throw new NotImplementedException();
        }

        public void ParseNoId(YamlMappingNode yamlNode)
        {
            var properties = EveYamlFactory.GetProperties(GetType());
            foreach (var node in yamlNode.Children)
            {
                var found = properties.FirstOrDefault(x => x.Name.GetAttr<InvItem>() == node.Key.ToString());
                if (found != null)
                {
                    found.SetValue(this, node.Value.ToString());
                }
            }
        }

        public bool IsRegion => TypeID == "3";
        public bool IsConstellation => TypeID == "4";
        public bool IsSolarSystem => TypeID == "5";
        public bool IsCorporation => TypeID == "2";

        public void FillNames(IEnumerable<InvUniqueName> items)
        {
            var foundItem = items.FirstOrDefault(x => x.Id == Id);
            if (foundItem != null)
            {
                Name = foundItem.Name;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class InvUniqueName : IYamlEntity
    {
        [JsonPropertyName("itemID")]
        public string Id { get; set; }

        [JsonPropertyName("groupID")]
        public string GroupID { get; set; }

        [JsonPropertyName("itemName")]
        public string Name { get; set; }

        public InvUniqueName()
        {
            Id = string.Empty;
            GroupID = string.Empty;
            Name = string.Empty;
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            throw new NotImplementedException();
        }

        public void ParseNoId(YamlMappingNode yamlNode)
        {
            var properties = EveYamlFactory.GetProperties(GetType());
            foreach (var node in yamlNode.Children)
            {
                var found = properties.FirstOrDefault(x => x.Name.GetAttr<InvUniqueName>() == node.Key.ToString());
                if (found != null)
                {
                    found.SetValue(this, node.Value.ToString());
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
