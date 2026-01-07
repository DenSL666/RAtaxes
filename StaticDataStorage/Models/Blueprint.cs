using EveSdeModel.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StaticDataStorage.Models
{
    [Table("Blueprints")]
    public class Blueprint : BaseModel
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Key { get; set; }

        /// <summary>
        /// Id чертежа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Максимальное количество прогонов чертежа.
        /// </summary>
        public int MaxProductionLimit { get; set; }

        /// <summary>
        /// Является ли результат производства чертежа опубликованным (доступным игрокам).
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Время создания копии из оригинала.
        /// </summary>
        public int CopyTime { get; set; }

        /// <summary>
        /// Время улучшения оригинала по эффективности расхода материалов.
        /// </summary>
        public int ResearchMaterialEfficientTime { get; set; }

        /// <summary>
        /// Время улучшения оригинала по эффективности расхода времени.
        /// </summary>
        public int ResearchTimeEfficientTime { get; set; }


        #region Reaction params

        /// <summary>
        /// Время создания продуктов формулы реакции.
        /// </summary>
        public int ReactionTime { get; set; }

        /// <summary>
        /// Упакованная строка продуктов формулы реакции.
        /// </summary>
        [Column("ReactionProducts")]
        public string ReactionProductsStr { get; private set; }

        [NotMapped]
        private IReadOnlyList<BaseProduct> _reactionProducts;
        [NotMapped]
        public IReadOnlyList<BaseProduct> ReactionProducts
        {
            get => _reactionProducts;
            private set
            {
                _reactionProducts = value;
                ReactionProductsStr = Pack(value);
            }
        }

        /// <summary>
        /// Упакованная строка материалов создания формулы реакции.
        /// </summary>
        [Column("ReactionMaterials")]
        public string ReactionMaterialsStr { get; private set; }

        [NotMapped]
        private IReadOnlyList<BaseMaterial> _reactionMaterials;
        [NotMapped]
        public IReadOnlyList<BaseMaterial> ReactionMaterials
        {
            get => _reactionMaterials;
            private set
            {
                _reactionMaterials = value;
                ReactionMaterialsStr = Pack(value);
            }
        }

        #endregion

        #region Manufacturing params

        /// <summary>
        /// Время производства продукта.
        /// </summary>
        public int ManufacturingTime { get; set; }

        /// <summary>
        /// Упакованная строка создания продуктов чертежа.
        /// </summary>
        [Column("ManufacturingProducts")]
        public string ManufacturingProductsStr { get; private set; }

        [NotMapped]
        private IReadOnlyList<BaseProduct> _manufacturingProducts;
        [NotMapped]
        public IReadOnlyList<BaseProduct> ManufacturingProducts
        {
            get => _manufacturingProducts;
            private set
            {
                _manufacturingProducts = value;
                ManufacturingProductsStr = Pack(value);
            }
        }

        /// <summary>
        /// Упакованная строка материалов создания продукции.
        /// </summary>
        [Column("ManufacturingMaterials")]
        public string ManufacturingMaterialsStr { get; private set; }

        [NotMapped]
        private IReadOnlyList<BaseMaterial> _manufacturingMaterials;
        [NotMapped]
        public IReadOnlyList<BaseMaterial> ManufacturingMaterials
        {
            get => _manufacturingMaterials;
            private set
            {
                _manufacturingMaterials = value;
                ManufacturingMaterialsStr = Pack(value);
            }
        }

        #endregion

        #region Invention Params

        /// <summary>
        /// Время превращения т1 копии в т2 копию.
        /// </summary>
        public int InventionTime { get; set; }

        /// <summary>
        /// Упакованная строка результатов превращения чертежа.
        /// </summary>
        [Column("InventionProducts")]
        public string InventionProductsStr { get; private set; }

        [NotMapped]
        private IReadOnlyList<BaseProduct> _inventionProducts;
        [NotMapped]
        public IReadOnlyList<BaseProduct> InventionProducts
        {
            get => _inventionProducts;
            private set
            {
                _inventionProducts = value;
                InventionProductsStr = Pack(value);
            }
        }

        /// <summary>
        /// Упакованная строка материалов, необходимых для превращения чертежа.
        /// </summary>
        [Column("InventionMaterials")]
        public string InventionMaterialsStr { get; private set; }

        [NotMapped]
        private IReadOnlyList<BaseMaterial> _inventionMaterials;
        [NotMapped]
        public IReadOnlyList<BaseMaterial> InventionMaterials
        {
            get => _inventionMaterials;
            private set
            {
                _inventionMaterials = value;
                InventionMaterialsStr = Pack(value);
            }
        }

        #endregion

        public static Blueprint Empty()
        {
            return new Blueprint()
            {
                Id = -1,
                MaxProductionLimit = -1,
                Published = false,
                CopyTime = -1,
                ResearchMaterialEfficientTime = -1,
                ResearchTimeEfficientTime = -1,

                ReactionTime = -1,
                ReactionProducts = new List<BaseProduct>(),
                ReactionMaterials = new List<BaseMaterial>(),

                ManufacturingTime = -1,
                ManufacturingProducts = new List<BaseProduct>(),
                ManufacturingMaterials = new List<BaseMaterial>(),

                InventionTime = -1,
                InventionProducts = new List<BaseProduct>(),
                InventionMaterials = new List<BaseMaterial>(),
            };
        }

        public void FillFrom(EveSdeModel.Models.Blueprint blueprint)
        {
            this.Id = int.Parse(blueprint.Id);
            this.Published = blueprint.IsPublished;
            this.MaxProductionLimit = int.Parse(blueprint.MaxProductionLimit);

            if (blueprint.Activities != null)
            {
                if (blueprint.Activities.Copying != null
                && !string.IsNullOrEmpty(blueprint.Activities.Copying.Time)
                && int.TryParse(blueprint.Activities.Copying.Time, out int _timeCopy))
                    this.CopyTime = _timeCopy;

                if (blueprint.Activities.ResearchTime != null
                    && !string.IsNullOrEmpty(blueprint.Activities.ResearchTime.Time)
                    && int.TryParse(blueprint.Activities.ResearchTime.Time, out int _timeResearchTime))
                    this.ResearchTimeEfficientTime = _timeResearchTime;

                if (blueprint.Activities.ResearchMaterial != null
                    && !string.IsNullOrEmpty(blueprint.Activities.ResearchMaterial.Time)
                    && int.TryParse(blueprint.Activities.ResearchMaterial.Time, out int _timeResearchMaterial))
                    this.ResearchMaterialEfficientTime = _timeResearchMaterial;

                if (blueprint.Activities.Reaction != null
                    && !string.IsNullOrEmpty(blueprint.Activities.Reaction.Time)
                    && int.TryParse(blueprint.Activities.Reaction.Time, out int _timeReaction))
                {
                    this.ReactionTime = _timeReaction;
                    this.ReactionMaterials = blueprint.Activities.Reaction.Materials.Select(x => x).ToList();
                    this.ReactionProducts = blueprint.Activities.Reaction.Products.Select(x => x).ToList();
                }

                if (blueprint.Activities.Manufacturing != null
                    && !string.IsNullOrEmpty(blueprint.Activities.Manufacturing.Time)
                    && int.TryParse(blueprint.Activities.Manufacturing.Time, out int _timeManufacturing))
                {
                    this.ManufacturingTime = _timeManufacturing;
                    this.ManufacturingMaterials = blueprint.Activities.Manufacturing.Materials.Select(x => x).ToList();
                    this.ManufacturingProducts = blueprint.Activities.Manufacturing.Products.Select(x => x).ToList();
                }

                if (blueprint.Activities.Invention != null
                    && !string.IsNullOrEmpty(blueprint.Activities.Invention.Time)
                    && int.TryParse(blueprint.Activities.Invention.Time, out int _timeInvention))
                {
                    this.InventionTime = _timeInvention;
                    this.InventionMaterials = blueprint.Activities.Invention.Materials.Select(x => x).ToList();
                    this.InventionProducts = blueprint.Activities.Invention.Products.Select(x => x).ToList();
                }
            }
        }

        public void LoadCollections()
        {
            this.ManufacturingMaterials = UnPack<BaseMaterial>(ManufacturingMaterialsStr).ToList();
            this.ManufacturingProducts = UnPack<BaseProduct>(ManufacturingMaterialsStr).ToList();

            this.ReactionMaterials = UnPack<BaseMaterial>(ReactionMaterialsStr).ToList();
            this.ReactionProducts = UnPack<BaseProduct>(ReactionMaterialsStr).ToList();

            this.InventionMaterials = UnPack<BaseMaterial>(InventionMaterialsStr).ToList();
            this.InventionProducts = UnPack<BaseProduct>(InventionMaterialsStr).ToList();
        }

        public void FillMaterials(IReadOnlyCollection<EntityType> items)
        {
            var foundBp = items.FirstOrDefault(x => x.Id == Id);
            if (foundBp != null)
            {
                Published = foundBp.Published;
            }

            ManufactoryMaterials = ManufactoryMaterials ?? new Dictionary<EntityType, string>();
            Products = Products ?? new Dictionary<EntityType, string>();

            if (ManufacturingMaterials != null)
            {
                foreach (var material in ManufacturingMaterials)
                {
                    var found = items.FirstOrDefault(x => x.Id == material.IntId);
                    if (found != null)
                    {
                        ManufactoryMaterials.Add(found, material.Quantity);
                    }
                }
            }

            if (ManufacturingProducts != null)
            {
                foreach (var product in ManufacturingProducts)
                {
                    var found = items.FirstOrDefault(x => x.Id == product.IntId);
                    if (found != null)
                    {
                        Products.Add(found, product.Quantity);
                    }
                }
            }

            if (ReactionMaterials != null)
            {
                foreach (var material in ReactionMaterials)
                {
                    var found = items.FirstOrDefault(x => x.Id == material.IntId);
                    if (found != null)
                    {
                        ManufactoryMaterials.Add(found, material.Quantity);
                    }
                }
            }

            if (ReactionProducts != null)
            {
                foreach (var product in ReactionProducts)
                {
                    var found = items.FirstOrDefault(x => x.Id == product.IntId);
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
        [NotMapped]
        public Dictionary<EntityType, string> ManufactoryMaterials { get; private set; }

        /// <summary>
        /// Словарь, описывающий какая продукция и в каком количестве будет создано в результате одного прогона.
        /// </summary>
        [NotMapped]
        public Dictionary<EntityType, string> Products { get; private set; }

        /// <summary>
        /// Зачастую продукция это один тип предмета.
        /// </summary>
        [NotMapped]
        public EntityType Product => Products.Keys.FirstOrDefault();

        /// <summary>
        /// Возможно ли создать с помощью чертежа что-либо доступное игрокам.
        /// </summary>
        [NotMapped]
        public bool HasManufactory => 
            (ManufacturingProducts != null && ManufacturingProducts.Any() 
            || ReactionProducts != null && ReactionProducts.Any()) 
            && Product != null && Product.Published;

        /// <summary>
        /// Создаёт ли чертеж топливные блоки.
        /// </summary>
        [NotMapped]
        public bool IsFuelBlock => Product != null && Product.Published && Product.NameEnglish.ToLower().Contains("fuel block");

        /// <summary>
        /// Является ли чертеж производственным.
        /// </summary>
        [NotMapped]
        public bool IsPrint => ManufacturingProducts != null && ManufacturingProducts.Any();

        /// <summary>
        /// Является ли чертеж формулой реакции.
        /// </summary>
        [NotMapped]
        public bool IsFormula => ReactionProducts != null && ReactionProducts.Any();

        /// <summary>
        /// Строковый формат чертежа, используемый в скрипте гугл таблицы.
        /// </summary>
        /// <returns></returns>
        public string Write() => $"  new Blueprint(\"{Product.NameEnglish.Replace("'", "").Replace("’", "")}\", {Products[Product]}, \"{(IsFormula ? "Formula" : $"{Product.Group?.Category?.Name} {Product.GetTech()}")}\", \"{string.Join("$", ManufactoryMaterials.Select(p => $"{p.Key.NameEnglish}&{p.Value}"))}\"),";
    }
}
