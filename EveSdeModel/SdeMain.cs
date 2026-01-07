using EveSdeModel.Factories;
using EveSdeModel.Models;
using EveSdeModel.Models.Id;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveSdeModel
{
    public class SdeMain
    {
        /// <summary>
        /// Путь к файлу SDE категорий сущностей.
        /// </summary>
        string CategoriesPath { get; }
        /// <summary>
        /// Путь к файлу SDE групп сущностей.
        /// </summary>
        string GroupsPath { get; }
        /// <summary>
        /// Путь к файлу SDE id и описанию сущностей.
        /// </summary>
        string TypeIDPath { get; }
        /// <summary>
        /// Путь к файлу SDE результату переработки сущностей.
        /// </summary>
        string TypeMaterialsPath { get; }
        /// <summary>
        /// Путь к файлу SDE чертежей.
        /// </summary>
        string BlueprintsPath { get; }
        /// <summary>
        /// Путь к файлу SDE уникальных имён.
        /// </summary>
        string InvUniqueNamesPath { get; }
        /// <summary>
        /// Путь к файлу SDE уникальных сущностей.
        /// </summary>
        string InvItemsPath { get; }
        /// <summary>
        /// Путь к файлу SDE регионов.
        /// </summary>
        string RegionsPath { get; }
        /// <summary>
        /// Путь к файлу SDE солнечных систем.
        /// </summary>
        string SolarSystemsPath { get; }

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
        /// Список сущностей SDE, описывающих системы, созвездия, регионы.
        /// </summary>
        public ReadOnlyCollection<InvItem> InvItems { get; private set; }
        /// <summary>
        /// Список уникальных имён SDE.
        /// </summary>
        public ReadOnlyCollection<InvUniqueName> InvUniqueNames { get; private set; }
        /// <summary>
        /// Список сущностей SDE, описывающих регионы.
        /// </summary>
        public ReadOnlyCollection<Region> Regions { get; private set; }
        /// <summary>
        /// Список сущностей SDE, описывающих системы.
        /// </summary>
        public ReadOnlyCollection<SolarSystem> SolarSystems { get; private set; }

        public SdeMain(IConfiguration configuration)
        {
            CategoriesPath = Path.Combine(AppContext.BaseDirectory, configuration.GetValue<string>("Runtime:PathSdeCategories"));
            GroupsPath = Path.Combine(AppContext.BaseDirectory, configuration.GetValue<string>("Runtime:PathSdeGroups"));
            TypeIDPath = Path.Combine(AppContext.BaseDirectory, configuration.GetValue<string>("Runtime:PathSdeTypes"));
            TypeMaterialsPath = Path.Combine(AppContext.BaseDirectory, configuration.GetValue<string>("Runtime:PathSdeTypeMaterials"));
            BlueprintsPath = Path.Combine(AppContext.BaseDirectory, configuration.GetValue<string>("Runtime:PathSdeBlueprints"));

            var downloadPath = configuration.GetValue<string>("Runtime:PathSdeDownload");

            InvUniqueNamesPath = Path.Combine(AppContext.BaseDirectory, downloadPath, "invUniqueNames.yaml");
            InvItemsPath = Path.Combine(AppContext.BaseDirectory, downloadPath, "invItems.yaml");
            RegionsPath = Path.Combine(AppContext.BaseDirectory, downloadPath, "mapRegions.yaml");
            SolarSystemsPath = Path.Combine(AppContext.BaseDirectory, downloadPath, "mapSolarSystems.yaml");

            Categories = new ReadOnlyCollection<Category>([]);
            Groups = new ReadOnlyCollection<Group>([]);
            Blueprints = new ReadOnlyCollection<Blueprint>([]);
            EntityTypes = new ReadOnlyCollection<EntityType>([]);
            TypeMaterials = new ReadOnlyCollection<TypeMaterial>([]);
            InvItems = new ReadOnlyCollection<InvItem>([]);
            InvUniqueNames = new ReadOnlyCollection<InvUniqueName>([]);
            Regions = new ReadOnlyCollection<Region>([]);
            SolarSystems = new ReadOnlyCollection<SolarSystem>([]);

            InitGroups();
            InitTypes();
            InitMaterials();

            //var file = new FileInfo(TypeIDPath);
            //var sizeBytes = file.Length;
            //var sizeMBytes = (double)sizeBytes / 1024 / 1024;

            ///// Если файл с id и описанием предметов слишком большой (по умолчанию он около 150 МБ)
            ///// То его нужно прочитать и сохранить, чтобы убрать лишние данные
            //if (sizeMBytes > 20)
            //    TryRewriteTypesSde();
        }


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

        /// <summary>
        /// Читает из файлов SDE категории и группы. Заполняет категорию каждой группы.
        /// </summary>
        public void InitGroups()
        {
            if (!Categories.Any() || !Groups.Any())
            {
                if (File.Exists(CategoriesPath) && File.Exists(GroupsPath))
                {
                    Categories = new ReadOnlyCollection<Category>(EveYamlFactory.ParseFile<Category>(CategoriesPath));
                    Groups = new ReadOnlyCollection<Group>(EveYamlFactory.ParseFile<Group>(GroupsPath));
                    foreach (var group in Groups)
                        group.FillCategories(Categories);
                }
            }
        }

        /// <summary>
        /// Читает из файла SDE типы объектов. Заполняет группу для каждого типа.
        /// </summary>
        public void InitTypes()
        {
            if (!Categories.Any() || !Groups.Any())
            {
                InitGroups();
            }
            if (!EntityTypes.Any())
            {
                if (File.Exists(TypeIDPath))
                {
                    EntityTypes = new ReadOnlyCollection<EntityType>(EveYamlFactory.ParseFile<EntityType>(TypeIDPath));
                    foreach (var type in EntityTypes)
                        type.FillGroups(Groups);
                }
            }
        }

        /// <summary>
        /// Читает из SDE списки переработки одних объектов в другие. Заполняет материалы типами объектов.
        /// </summary>
        public void InitMaterials()
        {
            if (!EntityTypes.Any())
            {
                InitTypes();
            }
            if (!TypeMaterials.Any())
            {
                if (File.Exists(TypeMaterialsPath))
                {
                    TypeMaterials = new ReadOnlyCollection<TypeMaterial>(EveYamlFactory.ParseFile<TypeMaterial>(TypeMaterialsPath));
                    foreach (var material in TypeMaterials)
                        material.FillMaterials(EntityTypes);
                }
            }
        }

        /// <summary>
        /// Читает из SDE списки производства объектов. Заполняет материалы типами объектов.
        /// </summary>
        public void InitBlueprints()
        {
            if (!EntityTypes.Any())
            {
                InitTypes();
            }
            if (!Blueprints.Any())
            {
                if (File.Exists(BlueprintsPath))
                {
                    Blueprints = new ReadOnlyCollection<Blueprint>(EveYamlFactory.ParseFile<Blueprint>(BlueprintsPath));
                    foreach (var blueprint in Blueprints)
                        blueprint.FillMaterials(EntityTypes);
                }
            }
        }

        /// <summary>
        /// Читает из SDE имена уникальных планетарных объектов и принадлежность одних планетарных объектов к другим. Заполняет имена объектов.
        /// </summary>
        public void InitInvItems()
        {
            if (!InvItems.Any())
            {
                if (File.Exists(InvUniqueNamesPath) && File.Exists(InvItemsPath))
                {
                    InvUniqueNames = new ReadOnlyCollection<InvUniqueName>(EveYamlFactory.ParseFileSequence<InvUniqueName>(InvUniqueNamesPath));
                    InvItems = new ReadOnlyCollection<InvItem>(EveYamlFactory.ParseFileSequence<InvItem>(InvItemsPath));
                    foreach (var invItem in InvItems.Where(x => x.IsSolarSystem || x.IsConstellation || x.IsRegion))
                        invItem.FillNames(InvUniqueNames);
                }
            }
        }

        /// <summary>
        /// Читает из SDE данные уникальных планетарных объектов.
        /// </summary>
        public void InitRegionsAndSolarSystems()
        {
            if (!Regions.Any())
            {
                if (File.Exists(RegionsPath))
                {
                    Regions = new ReadOnlyCollection<Region>(EveYamlFactory.ParseFile<Region>(RegionsPath));
                }
            }

            if (!SolarSystems.Any())
            {
                if (File.Exists(SolarSystemsPath))
                {
                    SolarSystems = new ReadOnlyCollection<SolarSystem>(EveYamlFactory.ParseFile<SolarSystem>(SolarSystemsPath));
                }
            }
        }

        /// <summary>
        /// Перезаписывает имеющийся файл SDE типов объектов на ранее прочитанный с целью сокращения занимаего места.
        /// </summary>
        private void TryRewriteTypesSde()
        {
            //  добавить кеширование контрольной суммы и сравнивать с ней
            var dgd2 = string.Join("", EntityTypes.Select(EveSdeModel.Serialization.EntityTypeConverter.SerializeEntityType));
            System.IO.File.WriteAllText(TypeIDPath, dgd2);
        }
    }
}
