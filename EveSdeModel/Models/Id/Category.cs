using EveSdeModel.Interfaces;
using EveSdeModel.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YamlDotNet.RepresentationModel;
using System.Text.Json.Serialization;

namespace EveSdeModel.Models
{
    public class Category : IYamlEntity
    {
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public Name Name { get; set; }

        [JsonPropertyName("published")]
        public string Published { get; set; }

        [JsonPropertyName("iconID")]
        public string IconID { get; set; }

        public bool IsPublished => !string.IsNullOrEmpty(Published) && Published == "true";

        public Category()
        {
            Id = string.Empty;
            Published = string.Empty;
            IconID = string.Empty;
            Name = new Name();
        }

        protected Category(Category category)
        {
            Id = new string(category.Id.ToCharArray());
            Published = new string(category.Published.ToCharArray());
            IconID = new string(category.IconID.ToCharArray());
            Name = category.Name.DeepCopy();
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            Id = yamlNode.Key.ToString();
            var properties = EveYamlFactory.GetProperties(GetType());
            foreach (var node in ((YamlMappingNode)yamlNode.Value).Children)
            {
                var found = properties.FirstOrDefault(x => x.Name.GetAttr<Category>() == node.Key.ToString());
                if (found != null)
                {
                    found.SetValue(this, node.Value.ToString());
                }
                if (node.Key.ToString() == nameof(Name).GetAttr<Category>())
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
            return Name?.English ?? "";
        }

        public static Category DeepCopy(Category category)
        {
            return new Category(category);
        }

        public Category DeepCopy()
        {
            return new Category(this);
        }
    }
}
