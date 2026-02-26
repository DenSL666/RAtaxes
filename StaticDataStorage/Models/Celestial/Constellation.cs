using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace StaticDataStorage.Models.Celestial
{
    /// <summary>
    /// Уникальная сущность созвездия.
    /// </summary>
    [Table("Constellations")]
    public class Constellation
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Key { get; set; }

        /// <summary>
        /// Id созвездия.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя созвездия.
        /// </summary>
        public string Name { get; set; }

        public int FactionID { get; set; }
        public int WormholeClassID { get; set; }

        /// <summary>
        /// Id региона, где находится созвездие.
        /// </summary>
        public int RegionID { get; set; }

        /// <summary>
        /// Строковая запись Id систем в созвездии.
        /// </summary>
        public string SolarSystemIDs { get; set; }

        /// <summary>
        /// Коллекция Id систем в созвездии.
        /// </summary>
        [NotMapped]
        public List<int> SolarSystemIdCollection { get; private set; }

        [NotMapped]
        public Region Region { get; private set; }

        public override string ToString()
        {
            return Name;
        }

        public static Constellation Empty()
        {
            return new Constellation()
            {
                Id = -1,
                FactionID = -1,
                WormholeClassID = -1,
                RegionID = -1,
                Name = string.Empty,
                SolarSystemIDs = string.Empty,
            };
        }

        public void FillFrom(EveSdeModel.Models.Id.Constellation constellation)
        {
            if (int.TryParse(constellation.Id, out int _id)) Id = _id;
            if (int.TryParse(constellation.FactionID, out int _factionID)) FactionID = _factionID;
            if (int.TryParse(constellation.WormholeClassID, out int _wormholeClassID)) WormholeClassID = _wormholeClassID;
            if (int.TryParse(constellation.RegionID, out int _regionID)) RegionID = _regionID;

            Name = constellation.Name.English;
            SolarSystemIDs = string.Join(";", constellation.SolarSystemIDs);
        }

        public void LoadCollections(IEnumerable<Region> regions)
        {
            if (string.IsNullOrEmpty(SolarSystemIDs))
                SolarSystemIdCollection = new List<int>();
            else
                SolarSystemIdCollection = SolarSystemIDs.Split(';').Select(int.Parse).ToList();
            var found = regions.FirstOrDefault(x => x.Id == RegionID);
            if (found != null)
            {
                Region = found;
            }
        }
    }
}
