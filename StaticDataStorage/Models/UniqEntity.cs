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
    /// <summary>
    /// Уникальная сущность, вроде региона, системы, НПЦ корпорации, НПЦ станции.
    /// </summary>
    [Table("UniqEntities")]
    public class UniqEntity
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Key { get; set; }

        public int Id { get; set; }

        public int FlagID { get; set; }

        public int LocationID { get; set; }

        public int OwnerID { get; set; }

        public long Quantity { get; set; }

        public int TypeID { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Является ли сущность регионом.
        /// </summary>
        [NotMapped]
        public bool IsRegion => TypeID == 3;
        /// <summary>
        /// Является ли сущность созвездием.
        /// </summary>
        [NotMapped]
        public bool IsConstellation => TypeID == 4;
        /// <summary>
        /// Является ли сущность системой.
        /// </summary>
        [NotMapped]
        public bool IsSolarSystem => TypeID == 5;
        /// <summary>
        /// Является ли сущность НПЦ корпорацией.
        /// </summary>
        [NotMapped]
        public bool IsCorporation => TypeID == 2;

        /// <summary>
        /// Заполняет поле имени сущности из списка имён уникальных сущностей.
        /// </summary>
        /// <param name="items">Список всех имён.</param>
        public void FillNames(IReadOnlyCollection<UniqName> items)
        {
            var foundItem = items.FirstOrDefault(x => x.Id == Id);
            if (foundItem != null)
            {
                Name = foundItem.Name;
            }
        }

        public static UniqEntity Empty()
        {
            return new UniqEntity()
            {
                Id = -1,
                FlagID = -1,
                LocationID = -1,
                OwnerID = -1,
                Quantity = -1,
                TypeID = -1,
                Name = string.Empty,
            };
        }

        public void FillFrom(EveSdeModel.Models.Id.InvItem invItem)
        {
            if (int.TryParse(invItem.Id, out int _id)) Id = _id;
            if (int.TryParse(invItem.FlagID, out int _flagID)) FlagID = _flagID;
            if (int.TryParse(invItem.LocationID, out int _locationID)) LocationID = _locationID;
            if (int.TryParse(invItem.OwnerID, out int _ownerID)) OwnerID = _ownerID;
            if (int.TryParse(invItem.Quantity, out int _quantity)) Quantity = _quantity;
            if (int.TryParse(invItem.TypeID, out int _typeID)) TypeID = _typeID;
            this.Name = invItem.Name;
        }
    }
}
