using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace StaticDataStorage.Models.Celestial
{
    /// <summary>
    /// Уникальная сущность солнечной системы.
    /// </summary>
    [Table("SolarSystems")]
    public class SolarSystem
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Key { get; set; }

        /// <summary>
        /// Id солнечной системы.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя солнечной системы.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Id созвездия, где находится система.
        /// </summary>
        public int ConstellationID { get; set; }

        /// <summary>
        /// Id региона, где находится система.
        /// </summary>
        public int RegionID { get; set; }

        public double SecurityStatus { get; set; }

        /// <summary>
        /// Id звезды в системе.
        /// </summary>
        public int StarID { get; set; }

        /// <summary>
        /// Строковая запись Id планет в системе.
        /// </summary>
        public string PlanetIDs { get; set; }

        /// <summary>
        /// Коллекция Id планет в системе.
        /// </summary>
        [NotMapped]
        public List<int> PlanetIdCollection { get; private set; }

        [NotMapped]
        public Constellation Constellation { get; private set; }

        [NotMapped]
        public Region Region { get; private set; }

        public override string ToString()
        {
            return Name;
        }

        public static SolarSystem Empty()
        {
            return new SolarSystem()
            {
                Id = -1,
                ConstellationID = -1,
                SecurityStatus = -1,
                RegionID = -1,
                StarID = -1,
                Name = string.Empty,
                PlanetIDs = string.Empty,
            };
        }

        public void FillFrom(EveSdeModel.Models.Id.SolarSystem solarSystem)
        {
            if (int.TryParse(solarSystem.Id, out int _id)) Id = _id;
            if (int.TryParse(solarSystem.ConstellationID, out int _constellationID)) ConstellationID = _constellationID;
            if (double.TryParse(solarSystem.SecurityStatus, out double _securityStatus)) SecurityStatus = _securityStatus;
            if (int.TryParse(solarSystem.RegionID, out int _regionID)) RegionID = _regionID;
            if (int.TryParse(solarSystem.StarID, out int _starID)) StarID = _starID;

            Name = solarSystem.Name.English;
            PlanetIDs = string.Join(";", solarSystem.PlanetIDs);
        }

        public void LoadCollections(IEnumerable<Region> regions, IEnumerable<Constellation> constellations)
        {
            if (string.IsNullOrEmpty(PlanetIDs))
                PlanetIdCollection = new List<int>();
            else
                PlanetIdCollection = PlanetIDs.Split(';').Select(int.Parse).ToList();
            var found = regions.FirstOrDefault(x => x.Id == RegionID);
            if (found != null)
            {
                Region = found;
            }
            var found2 = constellations.FirstOrDefault(x => x.Id == ConstellationID);
            if (found2 != null)
            {
                Constellation = found2;
            }
        }
    }
}
