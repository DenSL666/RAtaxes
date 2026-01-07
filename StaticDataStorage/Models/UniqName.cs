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
    /// Имя уникальной сущности, вроде региона, системы, НПЦ корпорации, НПЦ станции.
    /// </summary>
    [Table("UniqNames")]
    public class UniqName
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Key { get; set; }

        public int Id { get; set; }

        public int GroupID { get; set; }

        public string Name { get; set; }

        public static UniqName Empty()
        {
            return new UniqName()
            {
                Id = -1,
                Name = string.Empty,
            };
        }

        public void FillFrom(EveSdeModel.Models.Id.InvUniqueName invItem)
        {
            if (int.TryParse(invItem.Id, out int _id)) Id = _id;
            if (int.TryParse(invItem.GroupID, out int _groupID)) GroupID = _groupID;
            this.Name = invItem.Name;
        }
    }
}
