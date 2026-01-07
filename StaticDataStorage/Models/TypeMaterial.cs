using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticDataStorage.Models
{
    /// <summary>
    /// Сущность описывает, на какие сущности может быть переработана некоторая сущность типа <see cref="EntityType"/>.
    /// </summary>
    [Table("TypeMaterials")]
    public class TypeMaterial : BaseModel
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Key { get; set; }

        /// <summary>
        /// Id сущности.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Упакованная строка материалов, на которые происходит переработка.
        /// </summary>
        [Column("InventionMaterials")]
        public string MaterialsStr { get; private set; }

        [NotMapped]
        private IReadOnlyList<EveSdeModel.Models.BaseMaterial> _materials;

        /// <summary>
        /// Список материалов, на которые разбирается сущность при 100% переработке.
        /// </summary>
        [NotMapped]
        public IReadOnlyList<EveSdeModel.Models.BaseMaterial> Materials
        {
            get => _materials;
            private set
            {
                _materials = value;
                MaterialsStr = Pack(value);
            }
        }

        #region Readonly Properties

        /// <summary>
        /// Словарь, содержащий данные о сущностях и их количестве, на которые разбирается сущность при 100% переработке.
        /// </summary>
        [NotMapped]
        public Dictionary<EntityType, string> RefineMaterials { get; private set; }

        /// <summary>
        /// Сущность, которая разбирается на материалы.
        /// </summary>
        [NotMapped]
        public EntityType? Entity { get; private set; }

        /// <summary>
        /// Является ли сущность рудой.
        /// </summary>
        [NotMapped]
        public bool IsAsteroid => Entity != null && Entity.Published && Entity.Group != null && Entity.Group.CategoryID == 25;

        /// <summary>
        /// Минимальное количество сущностей, которые могут быть разобраны на материалы.<br/>
        /// Например, руды разбираются не менее, чем 100 единиц.
        /// </summary>
        [NotMapped]
        public int PortionSize => Entity != null ? Entity.PortionSize : 0;

        /// <summary>
        /// Является ли сущность рудой и относится к группе ледяных руд.
        /// </summary>
        [NotMapped]
        public bool IsIce => IsAsteroid && Entity.Group.Id == 465;

        /// <summary>
        /// Является ли сущность рудой и относится к группе лунных руд R4.
        /// </summary>
        [NotMapped]
        public bool IsUbiquitousMoon4 => IsAsteroid && Entity.Group.Id == 1884;

        /// <summary>
        /// Является ли сущность рудой и относится к группе лунных руд R8.
        /// </summary>
        [NotMapped]
        public bool IsCommonMoon8 => IsAsteroid && Entity.Group.Id == 1920;

        /// <summary>
        /// Является ли сущность рудой и относится к группе лунных руд R16.
        /// </summary>
        [NotMapped]
        public bool IsUncommonMoon16 => IsAsteroid && Entity.Group.Id == 1921;

        /// <summary>
        /// Является ли сущность рудой и относится к группе лунных руд R32.
        /// </summary>
        [NotMapped]
        public bool IsRareMoon32 => IsAsteroid && Entity.Group.Id == 1922;

        /// <summary>
        /// Является ли сущность рудой и относится к группе лунных руд R64.
        /// </summary>
        [NotMapped]
        public bool IsExceptionalMoon64 => IsAsteroid && Entity.Group.Id == 1923;

        /// <summary>
        /// Является ли сущность рудой и относится к группе лунных руд (любых).
        /// </summary>
        [NotMapped]
        public bool IsMoon => IsAsteroid && (IsUbiquitousMoon4 || IsCommonMoon8 || IsUncommonMoon16 || IsRareMoon32 || IsExceptionalMoon64);

        /// <summary>
        /// Является ли сущность рудой и относится к группе астероидных руд.
        /// </summary>
        [NotMapped]
        public bool IsMineral => IsAsteroid && !IsMoon && !IsIce;

        #endregion

        public static TypeMaterial Empty()
        {
            return new TypeMaterial()
            {
                Id = -1,
                Materials = new List<EveSdeModel.Models.BaseMaterial>(),
                RefineMaterials = new Dictionary<EntityType, string>(),
            };
        }

        public void FillFrom(EveSdeModel.Models.TypeMaterial typeMaterial)
        {
            if (int.TryParse(typeMaterial.Id, out int _id)) Id = _id;
            Materials = typeMaterial.Materials;
        }

        public void LoadCollections()
        {
            this.Materials = UnPack<EveSdeModel.Models.BaseMaterial>(MaterialsStr).ToList();
        }

        /// <summary>
        /// Заполняет поле сущности по её Id и словарь сущностей, на которые происходит разбор.
        /// </summary>
        /// <param name="items">Список всех сущностей.</param>
        public void FillMaterials(IEnumerable<EntityType> items)
        {
            RefineMaterials = RefineMaterials ?? new Dictionary<EntityType, string>();
            var foundItem = items.FirstOrDefault(x => x.Id == Id);
            if (foundItem != null)
            {
                Entity = foundItem;
            }

            foreach (var material in Materials)
            {
                var found = items.FirstOrDefault(x => x.Id == material.IntId);
                if (found != null)
                {
                    RefineMaterials.Add(found, material.Quantity);
                }
            }
        }

        /// <summary>
        /// Выполняет переработку указанного числа материала с указанным процентом переработки
        /// </summary>
        /// <param name="count">Число материалов</param>
        /// <param name="efficency">Процент переработки</param>
        /// <param name="excess">Излишек материалов, оставшихся после переработки</param>
        /// <returns>Словарь сущностей переработки и их количество</returns>
        public Dictionary<EntityType, long> Refine(long count, double efficency, out long excess)
        {
            excess = 0;
            var result = new Dictionary<EntityType, long>();
            if (efficency > 0 && PortionSize > 0 && count >= PortionSize)
            {
                long multiple = count / PortionSize;
                excess = count % PortionSize;
                //  result = Materials.Sum(x => (int)Math.Floor(Math.Floor(x.Value * multiple * efficiency) * x.Price));
                foreach (var pair in RefineMaterials)
                {
                    if (long.TryParse(pair.Value, out long _value))
                    {
                        var key = pair.Key;
                        var _res = (long)Math.Floor(_value * multiple * efficency);
                        result.Add(key, _res);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Выполняет переработку указанного числа материала с указанным процентом переработки
        /// </summary>
        /// <param name="count">Число материалов</param>
        /// <param name="efficency">Процент переработки</param>
        /// <param name="excess">Излишек материалов, оставшихся после переработки</param>
        /// <returns>Словарь сущностей переработки и их количество</returns>
        public Dictionary<EntityType, long> Refine(string count, double efficency, out long excess)
        {
            excess = 0;
            if (long.TryParse(count, out long countInt))
                return Refine(countInt, efficency, out excess);
            return new Dictionary<EntityType, long>();
        }

        public override string ToString()
        {
            return Entity != null ? Entity.NameEnglish : string.Empty;
        }
    }
}
