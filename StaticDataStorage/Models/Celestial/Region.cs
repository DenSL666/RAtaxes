using EveCommon.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace StaticDataStorage.Models.Celestial
{
    /// <summary>
    /// Уникальная сущность региона.
    /// </summary>
    [Table("Regions")]
    public class Region
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Key { get; set; }

        /// <summary>
        /// Id региона.
        /// </summary>
        public int Id { get; set; }
        public int FactionID { get; set; }
        public int WormholeClassID { get; set; }

        /// <summary>
        /// Имя региона.
        /// </summary>
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public static Region Empty()
        {
            return new Region()
            {
                Id = -1,
                FactionID = -1,
                WormholeClassID = -1,
                Name = string.Empty,
            };
        }

        public void FillFrom(EveSdeModel.Models.Id.Region region)
        {
            if (int.TryParse(region.Id, out int _id)) Id = _id;
            if (int.TryParse(region.FactionID, out int _factionID)) FactionID = _factionID;
            if (int.TryParse(region.WormholeClassID, out int _wormholeClassID)) WormholeClassID = _wormholeClassID;

            if (region.Name != null)
            {
                Name = region.Name.English;
            }
        }
    }
}
