using EveCommon;
using Microsoft.Extensions.Logging;
using StaticDataStorage.Contexts;
using StaticDataStorage.Models;
using StaticDataStorage.Models.Celestial;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StaticDataStorage.Workers
{
    public class StaticDataStorageReader
    {
        protected ILogger<UpdateSdeLogic> _logger { get; }

        /// <summary>
        /// Список категорий SDE.
        /// </summary>
        public ReadOnlyCollection<Category> Categories { get; private set; }
        /// <summary>
        /// Список групп SDE.
        /// </summary>
        public ReadOnlyCollection<Group> Groups { get; private set; }
        /// <summary>
        /// Список чертежей SDE.
        /// </summary>
        public ReadOnlyCollection<Blueprint> Blueprints { get; private set; }
        /// <summary>
        /// Список предметов SDE.
        /// </summary>
        public ReadOnlyCollection<EntityType> EntityTypes { get; private set; }
        /// <summary>
        /// Список предметов SDE, которые могут быть переработаны и во что.
        /// </summary>
        public ReadOnlyCollection<TypeMaterial> TypeMaterials { get; private set; }
        /// <summary>
        /// Список регионов игры.
        /// </summary>
        public ReadOnlyCollection<Region> Regions { get; private set; }
        /// <summary>
        /// Список созвездий игры.
        /// </summary>
        public ReadOnlyCollection<Constellation> Constellations { get; private set; }
        /// <summary>
        /// Список систем игры.
        /// </summary>
        public ReadOnlyCollection<SolarSystem> SolarSystems { get; private set; }
        /// <summary>
        /// Список планет игры.
        /// </summary>
        public ReadOnlyCollection<Planet> Planets { get; private set; }
        /// <summary>
        /// Список лун игры.
        /// </summary>
        public ReadOnlyCollection<Moon> Moons { get; private set; }

        private List<TypeMaterial> _asteroid;
        /// <summary>
        /// Список сущностей, относящихся к группе руд.
        /// </summary>
        public List<TypeMaterial> Asteroid
        {
            get
            {
                if (_asteroid == null)
                    _asteroid = TypeMaterials.Where(x => x.IsAsteroid).ToList();
                return _asteroid;
            }
        }

        private List<EntityType> _asteroidRefineItems;
        /// <summary>
        /// Список сущностей, которые получаются в результате переработки руд.
        /// </summary>
        public List<EntityType> AsteroidRefineItems
        {
            get
            {
                if (_asteroidRefineItems == null)
                    _asteroidRefineItems = Asteroid.SelectMany(x => x.RefineMaterials.Keys).GroupBy(x => x.Id).Select(x => x.First()).ToList();
                return _asteroidRefineItems;
            }
        }

        public StaticDataStorageReader(ILogger<UpdateSdeLogic> logger)
        {
            _logger = logger;
            Categories = new ReadOnlyCollection<Category>([]);
            Groups = new ReadOnlyCollection<Group>([]);
            Blueprints = new ReadOnlyCollection<Blueprint>([]);
            EntityTypes = new ReadOnlyCollection<EntityType>([]);
            TypeMaterials = new ReadOnlyCollection<TypeMaterial>([]);

            Regions = new ReadOnlyCollection<Region>([]);
            Constellations = new ReadOnlyCollection<Constellation>([]);
            SolarSystems = new ReadOnlyCollection<SolarSystem>([]);
            Planets = new ReadOnlyCollection<Planet>([]);
            Moons = new ReadOnlyCollection<Moon>([]);

            try
            {
                using (var context = new StorageContext())
                {
                    Categories = new ReadOnlyCollection<Category>(context.Categories.ToArray());
                    Groups = new ReadOnlyCollection<Group>(context.Groups.ToArray());
                    Blueprints = new ReadOnlyCollection<Blueprint>(context.Blueprints.ToArray());
                    EntityTypes = new ReadOnlyCollection<EntityType>(context.EntityTypes.ToArray());
                    TypeMaterials = new ReadOnlyCollection<TypeMaterial>(context.TypeMaterials.ToArray());

                    Regions = new ReadOnlyCollection<Region>(context.Regions.ToArray());
                    Constellations = new ReadOnlyCollection<Constellation>(context.Constellations.ToArray());
                    SolarSystems = new ReadOnlyCollection<SolarSystem>(context.SolarSystems.ToArray());
                }
                foreach (var group in Groups)
                    group.FillCategories(Categories);
                foreach (var type in EntityTypes)
                    type.FillGroups(Groups);
                foreach (var material in TypeMaterials)
                {
                    material.LoadCollections();
                    material.FillMaterials(EntityTypes);
                }
                foreach (var blueprint in Blueprints)
                {
                    blueprint.LoadCollections();
                    blueprint.FillMaterials(EntityTypes);
                }

                foreach (var constellation in Constellations)
                    constellation.LoadCollections(Regions);
                foreach (var solarSystem in SolarSystems)
                    solarSystem.LoadCollections(Regions, Constellations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке чтения БД статичных данных.");
            }
        }

        public void LoadCelestials()
        {
            try
            {
                using (var context = new StorageContext())
                {
                    Planets = new ReadOnlyCollection<Planet>(context.Planets.ToArray());
                    Moons = new ReadOnlyCollection<Moon>(context.Moons.ToArray());
                }

                foreach (var planet in Planets)
                    planet.LoadCollections(SolarSystems);
                foreach (var moon in Moons)
                    moon.LoadCollections(SolarSystems, Planets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке чтения БД статичных данных.");
            }
        }

        public void CreateBlueprints(string savePathFileTypes, string savePathFileBlueprints)
        {
            if (!string.IsNullOrEmpty(savePathFileTypes))
            {
                var writeTypes = EntityTypes
                    .Where(x => x.Published
                                && !x.NameEnglish.Contains("SKIN")
                                && !x.NameEnglish.EndsWith("Blueprint")
                                && !x.NameEnglish.EndsWith("Emblem")
                                && !x.NameEnglish.EndsWith("Limited")
                                && !x.NameEnglish.EndsWith("Unlimited"))
                    .Select(x => $"  {x.Id}: \"{x.NameEnglish.Replace('\"', '\'')}\",").ToList();
                using (var wr = new StreamWriter(savePathFileTypes))
                {
                    foreach (var item in writeTypes)
                    {
                        wr.WriteLine(item);
                    }
                }
            }

            if (!string.IsNullOrEmpty(savePathFileBlueprints))
            {
                //фильтр и вывод блюпринтов
                var hasManu = Blueprints.Where(x => x.HasManufactory && !x.IsFuelBlock).ToList();
                var found = hasManu.FirstOrDefault(x => x.Product != null && x.Product.NameEnglish == "Tungsten Carbide" && x.MaxProductionLimit > 1000);
                if (found != null)
                    hasManu.Remove(found);
                using (var wr = new StreamWriter(savePathFileBlueprints))
                {
                    foreach (var bp in hasManu.OrderBy(x => x.Product.NameEnglish.Replace("'", "").Replace("’", "")))
                    {
                        wr.WriteLine(bp.Write());
                    }
                }
            }
        }

        public void SdeStartWork(string fileFullScanMoon, string fileSave)
        {
            if (!File.Exists(fileFullScanMoon))
                return;

            var lines = File.ReadAllLines(fileFullScanMoon);
            var list = lines.Select(x => x.Split('\t')).ToList();
            var moonNames = list.Select(x => x.First()).ToList();

            LoadCelestials();
            var solars = SolarSystems.Where(x => x.Region != null && x.Region.Name == "The Spire").ToArray();
            var planetIds = solars.SelectMany(x => x.PlanetIdCollection).ToArray();
            var planets = Planets.Where(x => planetIds.Contains(x.Id)).ToArray();
            var moonIds = planets.SelectMany(x => x.MoonIDCollection).ToArray();
            var moons = Moons.Where(x => moonIds.Contains(x.Id)).ToArray();

            var allNames = moons.Select(x => x.Name).ToArray();
            var except = allNames.Except(moonNames).ToArray();

            File.WriteAllLines(fileSave, except);
        }
    }
}
