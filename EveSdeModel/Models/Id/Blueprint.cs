using EveSdeModel.Factories;
using EveSdeModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;


namespace EveSdeModel.Models
{
    public class Blueprint : IYamlEntity
    {
        public string Id { get; set; }

        [JsonPropertyName("activities")]
        public Activities Activities { get; set; }

        [JsonPropertyName("blueprintTypeID")]
        public string BlueprintTypeID { get; set; }

        [JsonPropertyName("maxProductionLimit")]
        public string MaxProductionLimit { get; set; }

        public bool IsPublished { get; set; }


        public Blueprint()
        {
            Id = string.Empty;
            BlueprintTypeID = string.Empty;
            MaxProductionLimit = string.Empty;
            IsPublished = true;

            Activities = new Activities();
            ManufactoryMaterials = new Dictionary<EntityType, string>();
            Products = new Dictionary<EntityType, string>();
        }

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            Id = yamlNode.Key.ToString();
            var properties = EveYamlFactory.GetProperties(GetType());
            foreach (var node in ((YamlMappingNode)yamlNode.Value).Children)
            {
                var found = properties.FirstOrDefault(x => x.Name.GetAttr<Blueprint>() == node.Key.ToString());
                if (found != null)
                {
                    found.SetValue(this, node.Value.ToString());
                }
                if (node.Key.ToString() == nameof(Activities).GetAttr<Blueprint>())
                {
                    Activities = EveYamlFactory.GetObject<Activities>((YamlMappingNode)node.Value);
                }
            }
        }

        public void ParseNoId(YamlMappingNode yamlNode)
        {
            throw new NotImplementedException();
        }

        public void FillMaterials(IEnumerable<EntityType> items)
        {
            var foundBp = items.FirstOrDefault(x => x.Id == BlueprintTypeID);
            if (foundBp != null)
            {
                IsPublished = foundBp.IsPublished;
            }

            if (Activities != null && Activities.Manufacturing != null && Activities.Manufacturing.Materials != null)
            {
                foreach (var material in Activities.Manufacturing.Materials)
                {
                    var found = items.FirstOrDefault(x => x.Id == material.MaterialTypeID);
                    if (found != null)
                    {
                        ManufactoryMaterials.Add(found, material.Quantity);
                    }
                }
            }

            if (Activities != null && Activities.Manufacturing != null && Activities.Manufacturing.Products != null)
            {
                foreach (var product in Activities.Manufacturing.Products)
                {
                    var found = items.FirstOrDefault(x => x.Id == product.TypeID);
                    if (found != null)
                    {
                        Products.Add(found, product.Quantity);
                    }
                }
            }

            if (Activities != null && Activities.Reaction != null && Activities.Reaction.Materials != null)
            {
                foreach (var material in Activities.Reaction.Materials)
                {
                    var found = items.FirstOrDefault(x => x.Id == material.MaterialTypeID);
                    if (found != null)
                    {
                        ManufactoryMaterials.Add(found, material.Quantity);
                    }
                }
            }

            if (Activities != null && Activities.Reaction != null && Activities.Reaction.Products != null)
            {
                foreach (var product in Activities.Reaction.Products)
                {
                    var found = items.FirstOrDefault(x => x.Id == product.TypeID);
                    if (found != null)
                    {
                        Products.Add(found, product.Quantity);
                    }
                }
            }
        }

        public Dictionary<EntityType, string> ManufactoryMaterials { get; }
        public Dictionary<EntityType, string> Products { get; }
        public EntityType Product => Products.Keys.FirstOrDefault();

        public bool HasManufactory => Activities != null &&
            (Activities.Manufacturing != null && Activities.Manufacturing.Products.Any() ||
            Activities.Reaction != null && Activities.Reaction.Products.Any())
            && Product != null && Product.IsPublished && IsPublished;

        public bool IsFuelBlock => Product != null && Product.IsPublished && Product.Name.English.ToLower().Contains("fuel block");
        public bool IsPrint => Activities != null && Activities.Manufacturing != null && Activities.Manufacturing.Products != null && Activities.Manufacturing.Products.Any();
        public bool IsFormula => Activities != null && Activities.Reaction != null && Activities.Reaction.Products != null && Activities.Reaction.Products.Any();

        public string Write() => $"  new Blueprint(\"{Product.Name.English.Replace("'", "").Replace("’", "")}\", {Products[Product]}, \"{(IsFormula ? "Formula" : $"{Product.Group?.Category?.Name?.English} {Product.GetTech()}")}\", \"{string.Join("$", ManufactoryMaterials.Select(p => $"{p.Key.Name.English}&{p.Value}"))}\"),";
    }
}
