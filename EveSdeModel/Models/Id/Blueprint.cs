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
    /// <summary>
    /// Описывает способы использования чертежа.
    /// </summary>
    public class Blueprint : IYamlEntity
    {
        /// <summary>
        /// Id чертежа
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Контейнер, содержащий сведения о способах использования чертежа.
        /// </summary>
        [JsonPropertyName("activities")]
        public Activities Activities { get; set; }

        /// <summary>
        /// Id чертежа
        /// </summary>
        [JsonPropertyName("blueprintTypeID")]
        public string BlueprintTypeID { get; set; }

        /// <summary>
        /// Максимальное количество прогонов чертежа.
        /// </summary>
        [JsonPropertyName("maxProductionLimit")]
        public string MaxProductionLimit { get; set; }

        /// <summary>
        /// Является ли результат производства чертежа опубликованным (доступным игрокам).
        /// </summary>
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

        /// <summary>
        /// Словарь, описывающий какие материалы и в каком количестве используются при создании одного прогона продукции.
        /// </summary>
        public Dictionary<EntityType, string> ManufactoryMaterials { get; }
        /// <summary>
        /// Словарь, описывающий какая продукция и в каком количестве будет создано в результате одного прогона.
        /// </summary>
        public Dictionary<EntityType, string> Products { get; }
        /// <summary>
        /// Зачастую продукция это один тип предмета.
        /// </summary>
        public EntityType Product => Products.Keys.FirstOrDefault();

        /// <summary>
        /// Возможно ли создать с помощью чертежа что-либо доступное игрокам.
        /// </summary>
        public bool HasManufactory => Activities != null &&
            (Activities.Manufacturing != null && Activities.Manufacturing.Products.Any() ||
            Activities.Reaction != null && Activities.Reaction.Products.Any())
            && Product != null && Product.IsPublished && IsPublished;

        /// <summary>
        /// Создаёт ли чертеж топливные блоки.
        /// </summary>
        public bool IsFuelBlock => Product != null && Product.IsPublished && Product.Name.English.ToLower().Contains("fuel block");
        /// <summary>
        /// Является ли чертеж производственным.
        /// </summary>
        public bool IsPrint => Activities != null && Activities.Manufacturing != null && Activities.Manufacturing.Products != null && Activities.Manufacturing.Products.Any();
        /// <summary>
        /// Является ли чертеж формулой реакции.
        /// </summary>
        public bool IsFormula => Activities != null && Activities.Reaction != null && Activities.Reaction.Products != null && Activities.Reaction.Products.Any();

        /// <summary>
        /// Строковый формат чертежа, используемый в скрипте гугл таблицы.
        /// </summary>
        /// <returns></returns>
        public string Write() => $"  new Blueprint(\"{Product.Name.English.Replace("'", "").Replace("’", "")}\", {Products[Product]}, \"{(IsFormula ? "Formula" : $"{Product.Group?.Category?.Name?.English} {Product.GetTech()}")}\", \"{string.Join("$", ManufactoryMaterials.Select(p => $"{p.Key.Name.English}&{p.Value}"))}\"),";
    }
}
