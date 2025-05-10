using EveSdeModel.Factories;
using EveSdeModel.Models;
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
        string CategoriesPath { get; }
        string GroupsPath { get; }
        string TypeIDPath { get; }
        string TypeMaterialsPath { get; }
        string BlueprintsPath { get; }

        public ReadOnlyCollection<Category> Categories { get; private set; }
        public ReadOnlyCollection<Group> Groups { get; private set; }
        public ReadOnlyCollection<Blueprint> Blueprints { get; private set; }
        public ReadOnlyCollection<EntityType> EntityTypes { get; private set; }
        public ReadOnlyCollection<TypeMaterial> TypeMaterials { get; private set; }

        public SdeMain(IConfiguration configuration)
        {
            CategoriesPath = Path.Combine(AppContext.BaseDirectory, configuration.GetValue<string>("Runtime:PathSdeCategories"));
            GroupsPath = Path.Combine(AppContext.BaseDirectory, configuration.GetValue<string>("Runtime:PathSdeGroups"));
            TypeIDPath = Path.Combine(AppContext.BaseDirectory, configuration.GetValue<string>("Runtime:PathSdeTypes"));
            TypeMaterialsPath = Path.Combine(AppContext.BaseDirectory, configuration.GetValue<string>("Runtime:PathSdeTypeMaterials"));
            BlueprintsPath = Path.Combine(AppContext.BaseDirectory, configuration.GetValue<string>("Runtime:PathSdeBlueprints"));

            Categories = new ReadOnlyCollection<Category>([]);
            Groups = new ReadOnlyCollection<Group>([]);
            Blueprints = new ReadOnlyCollection<Blueprint>([]);
            EntityTypes = new ReadOnlyCollection<EntityType>([]);
            TypeMaterials = new ReadOnlyCollection<TypeMaterial>([]);

            InitGroups();
            InitTypes();
            InitMaterials();

            var file = new FileInfo(TypeIDPath);
            var sizeBytes = file.Length;
            var sizeMBytes = (double)sizeBytes / 1024 / 1024;

            if (sizeMBytes > 20)
                TryRewriteTypesSde();
        }


        private List<TypeMaterial> _asteroid;
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
        public List<EntityType> AsteroidRefineItems
        {
            get
            {
                if (_asteroidRefineItems == null)
                    _asteroidRefineItems = Asteroid.SelectMany(x => x.RefineMaterials.Keys).GroupBy(x => x.Id).Select(x => x.First()).ToList();
                return _asteroidRefineItems;
            }
        }

        public void InitGroups()
        {
            if (!Categories.Any() || !Groups.Any())
            {
                Categories = new ReadOnlyCollection<Category>(EveYamlFactory.ParseFile<Category>(CategoriesPath));
                Groups = new ReadOnlyCollection<Group>(EveYamlFactory.ParseFile<Group>(GroupsPath));
                foreach (var group in Groups)
                    group.FillCategories(Categories);
            }
        }

        public void InitTypes()
        {
            if (!Categories.Any() || !Groups.Any())
            {
                InitGroups();
            }
            if (!EntityTypes.Any())
            {
                EntityTypes = new ReadOnlyCollection<EntityType>(EveYamlFactory.ParseFile<EntityType>(TypeIDPath));
                foreach (var type in EntityTypes)
                    type.FillGroups(Groups);
            }
        }

        public void InitMaterials()
        {
            if (!EntityTypes.Any())
            {
                InitGroups();
            }
            if (!TypeMaterials.Any())
            {
                TypeMaterials = new ReadOnlyCollection<TypeMaterial>(EveYamlFactory.ParseFile<TypeMaterial>(TypeMaterialsPath));
                foreach (var material in TypeMaterials)
                    material.FillMaterials(EntityTypes);
            }
        }

        public void InitBlueprints()
        {
            if (!EntityTypes.Any())
            {
                InitGroups();
            }
            if (!Blueprints.Any())
            {
                Blueprints = new ReadOnlyCollection<Blueprint>(EveYamlFactory.ParseFile<Blueprint>(BlueprintsPath));
                foreach (var blueprint in Blueprints)
                    blueprint.FillMaterials(EntityTypes);
            }
        }

        private void TryRewriteTypesSde()
        {
            //  добавить кеширование контрольной суммы и сравнивать с ней
            var dgd2 = string.Join("", EntityTypes.Select(EveSdeModel.Serialization.EntityTypeConverter.SerializeEntityType));
            System.IO.File.WriteAllText(TypeIDPath, dgd2);
        }
    }
}
