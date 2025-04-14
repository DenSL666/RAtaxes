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
    public class Group : IYamlEntity
    {
        public string Id { get; set; }

        [JsonPropertyName("anchorable")]
        public string Anchorable { get; set; }

        [JsonPropertyName("anchored")]
        public string Anchored { get; set; }

        [JsonPropertyName("categoryID")]
        public string CategoryID { get; set; }

        [JsonPropertyName("fittableNonSingleton")]
        public string FittableNonSingleton { get; set; }

        [JsonPropertyName("iconID")]
        public string IconID { get; set; }

        [JsonPropertyName("name")]
        public Name Name { get; set; }

        [JsonPropertyName("published")]
        public string Published { get; set; }

        [JsonPropertyName("useBasePrice")]
        public string UseBasePrice { get; set; }

        public Category Category { get; private set; }
        public bool IsPublished => !string.IsNullOrEmpty(Published) && Published == "true";

        public Group()
        {
            Id = string.Empty;
            Anchorable = string.Empty;
            Anchored = string.Empty;
            CategoryID = string.Empty;
            FittableNonSingleton = string.Empty;
            IconID = string.Empty;
            Published = string.Empty;
            UseBasePrice = string.Empty;

            Category = new Category();
            Name = new Name();
        }

        protected Group(Group group)
        {
            Id = new string(group.Id.ToCharArray());
            Anchorable = new string(group.Anchorable.ToCharArray());
            Anchored = new string(group.Anchored.ToCharArray());
            CategoryID = new string(group.CategoryID.ToCharArray());
            FittableNonSingleton = new string(group.FittableNonSingleton.ToCharArray());
            IconID = new string(group.IconID.ToCharArray());
            Published = new string(group.Published.ToCharArray());
            UseBasePrice = new string(group.UseBasePrice.ToCharArray());

            Category = group.Category.DeepCopy();
            Name = group.Name.DeepCopy();
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            Id = yamlNode.Key.ToString();
            var properties = EveYamlFactory.GetProperties(GetType());
            foreach (var node in ((YamlMappingNode)yamlNode.Value).Children)
            {
                var found = properties.FirstOrDefault(x => x.Name.GetAttr<Group>() == node.Key.ToString());
                if (found != null)
                {
                    found.SetValue(this, node.Value.ToString());
                }
                if (node.Key.ToString() == nameof(Name).GetAttr<Group>())
                {
                    Name = EveYamlFactory.GetObject<Name>((YamlMappingNode)node.Value);
                }
            }
        }

        public void ParseNoId(YamlMappingNode yamlNode)
        {
            throw new NotImplementedException();
        }

        public void FillCategories(IEnumerable<Category> categories)
        {
            var found = categories.FirstOrDefault(x => x.Id == CategoryID);
            if (found != null)
            {
                Category = found;
            }
        }

        public override string ToString()
        {
            return Name?.English ?? "";
        }

        public static Group DeepCopy(Group group)
        {
            return new Group(group);
        }

        public Group DeepCopy()
        {
            return new Group(this);
        }
    }
}
