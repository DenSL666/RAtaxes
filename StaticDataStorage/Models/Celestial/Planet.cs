using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace StaticDataStorage.Models.Celestial
{
    /// <summary>
    /// Уникальная сущность планеты.
    /// </summary>
    [Table("Planets")]
    public class Planet
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Key { get; set; }

        /// <summary>
        /// Id планеты.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя планеты.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Номер планеты в системе.
        /// </summary>
        public int СelestialIndex { get; set; }

        /// <summary>
        /// Радиус планеты.
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// Id системы, где находится планета.
        /// </summary>
        public int SolarSystemID { get; set; }

        /// <summary>
        /// Id типа планеты.
        /// </summary>
        public int TypeID { get; set; }

        /// <summary>
        /// Строковая запись Id лун у планеты.
        /// </summary>
        public string MoonIDs { get; set; }

        /// <summary>
        /// Тип ресурса планеты для SOV.
        /// </summary>
        public int IdSovResource { get; set; }

        /// <summary>
        /// Количество ресурса планеты для SOV.
        /// </summary>
        public double SovResourceValue { get; set; }

        /// <summary>
        /// Коллекция Id лун у планеты.
        /// </summary>
        [NotMapped] 
        public List<int> MoonIDCollection { get; private set; }

        [NotMapped]
        public SolarSystem SolarSystem { get; private set; }

        public override string ToString()
        {
            return Name;
        }

        public static Planet Empty()
        {
            return new Planet()
            {
                Id = -1,
                СelestialIndex = -1,
                Radius = -1,
                SolarSystemID = -1,
                TypeID = -1,
                Name = string.Empty,
                MoonIDs = string.Empty,
                IdSovResource = -1,
                SovResourceValue = -1,
            };
        }

        public void FillFrom(EveSdeModel.Models.Id.Planet planet)
        {
            if (int.TryParse(planet.Id, out int _id)) Id = _id;
            if (int.TryParse(planet.СelestialIndex, out int _celestialIndex)) СelestialIndex = _celestialIndex;
            if (double.TryParse(planet.Radius, out double _radius)) Radius = _radius;
            if (int.TryParse(planet.SolarSystemID, out int _solarSystemID)) SolarSystemID = _solarSystemID;
            if (int.TryParse(planet.TypeID, out int _typeID)) TypeID = _typeID;

            Name = planet.Name;
            MoonIDs = string.Join(";", planet.MoonIDs);
        }

        public void LoadCollections(IEnumerable<SolarSystem> solarSystems)
        {
            if (string.IsNullOrEmpty(MoonIDs))
                MoonIDCollection = new List<int>();
            else
                MoonIDCollection = MoonIDs.Split(';').Select(int.Parse).ToList();
            var found = solarSystems.FirstOrDefault(x => x.Id == SolarSystemID);
            if (found != null)
            {
                SolarSystem = found;
            }
        }
    }
}

