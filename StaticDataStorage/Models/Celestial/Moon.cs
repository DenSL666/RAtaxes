using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace StaticDataStorage.Models.Celestial
{
    /// <summary>
    /// Уникальная сущность луны.
    /// </summary>
    [Table("Moons")]
    public class Moon
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Key { get; set; }

        /// <summary>
        /// Id луны.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя луны.
        /// </summary>
        public string Name { get; set; }

        public int СelestialIndex { get; set; }

        /// <summary>
        /// Номер луны у планеты.
        /// </summary>
        public int OrbitIndex { get; set; }

        /// <summary>
        /// Id планеты, у которой расположена луна.
        /// </summary>
        public int OrbitID { get; set; }

        /// <summary>
        /// Id системы, где находится луна.
        /// </summary>
        public int SolarSystemID { get; set; }

        /// <summary>
        /// Id типа луны.
        /// </summary>
        public int TypeID { get; set; }

        [NotMapped]
        public Planet Planet { get; private set; }

        [NotMapped]
        public SolarSystem SolarSystem { get; private set; }

        public override string ToString()
        {
            return Name;
        }

        public static Moon Empty()
        {
            return new Moon()
            {
                Id = -1,
                СelestialIndex = -1,
                OrbitID = -1,
                OrbitIndex = -1,
                SolarSystemID = -1,
                TypeID = -1,
                Name = string.Empty,
            };
        }

        public void FillFrom(EveSdeModel.Models.Id.Moon moon)
        {
            if (int.TryParse(moon.Id, out int _id)) Id = _id;
            if (int.TryParse(moon.СelestialIndex, out int _celestialIndex)) СelestialIndex = _celestialIndex;
            if (int.TryParse(moon.OrbitID, out int _orbitID)) OrbitID = _orbitID;
            if (int.TryParse(moon.OrbitIndex, out int _orbitIndex)) OrbitIndex = _orbitIndex;
            if (int.TryParse(moon.SolarSystemID, out int _solarSystemID)) SolarSystemID = _solarSystemID;
            if (int.TryParse(moon.TypeID, out int _typeID)) TypeID = _typeID;

            Name = moon.Name;
        }

        public void LoadCollections(IEnumerable<SolarSystem> solarSystems, IEnumerable<Planet> planets)
        {
            var found = solarSystems.FirstOrDefault(x => x.Id == SolarSystemID);
            if (found != null)
            {
                SolarSystem = found;
            }
            var found2 = planets.FirstOrDefault(x => x.Id == OrbitID);
            if (found2 != null)
            {
                Planet = found2;
            }
        }
    }
}

