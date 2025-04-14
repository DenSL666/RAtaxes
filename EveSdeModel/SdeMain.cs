using EveSdeModel.Factories;
using EveSdeModel.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveSdeModel
{
    public static class SdeMain
    {
        static string PathToFiles { get; } = @"sde";
        static string CategoriesFilename { get; } = @"categories.yaml";
        static string GroupsFilename { get; } = @"groups.yaml";
        static string TypeIDFilename { get; } = @"types.yaml";
        static string TypeMaterialsFilename { get; } = @"typeMaterials.yaml";
        static string BlueprintsFilename { get; } = @"blueprints.yaml";

        public static ReadOnlyCollection<Category> Categories { get; private set; }
        public static ReadOnlyCollection<Group> Groups { get; private set; }
        public static ReadOnlyCollection<Blueprint> Blueprints { get; private set; }
        public static ReadOnlyCollection<EntityType> EntityTypes { get; private set; }
        public static ReadOnlyCollection<TypeMaterial> TypeMaterials { get; private set; }

        static SdeMain()
        {
            Categories = new ReadOnlyCollection<Category>([]);
            Groups = new ReadOnlyCollection<Group>([]);
            Blueprints = new ReadOnlyCollection<Blueprint>([]);
            EntityTypes = new ReadOnlyCollection<EntityType>([]);
            TypeMaterials = new ReadOnlyCollection<TypeMaterial>([]);
        }

        public static void InitializeAll(bool needRewriteTypesSde = true)
        {
            InitGroups();
            InitTypes();
            InitMaterials();

            if (needRewriteTypesSde)
                TryRewriteTypesSde();
        }

        public static void InitGroups()
        {
            if (!Categories.Any() || !Groups.Any())
            {
                Categories = new ReadOnlyCollection<Category>(EveYamlFactory.ParseFile<Category>(Path.Combine(PathToFiles, CategoriesFilename)));
                Groups = new ReadOnlyCollection<Group>(EveYamlFactory.ParseFile<Group>(Path.Combine(PathToFiles, GroupsFilename)));
                foreach (var group in Groups)
                    group.FillCategories(Categories);
            }
        }

        public static void InitTypes()
        {
            if (!Categories.Any() || !Groups.Any())
            {
                InitGroups();
            }
            if (!EntityTypes.Any())
            {
                EntityTypes = new ReadOnlyCollection<EntityType>(EveYamlFactory.ParseFile<EntityType>(Path.Combine(PathToFiles, TypeIDFilename)));
                foreach (var type in EntityTypes)
                    type.FillGroups(Groups);
            }
        }

        public static void InitMaterials()
        {
            if (!EntityTypes.Any())
            {
                InitGroups();
            }
            if (!TypeMaterials.Any())
            {
                TypeMaterials = new ReadOnlyCollection<TypeMaterial>(EveYamlFactory.ParseFile<TypeMaterial>(Path.Combine(PathToFiles, TypeMaterialsFilename)));
                foreach (var material in TypeMaterials)
                    material.FillMaterials(EntityTypes);
            }
        }

        public static void InitBlueprints()
        {
            if (!EntityTypes.Any())
            {
                InitGroups();
            }
            if (!Blueprints.Any())
            {
                Blueprints = new ReadOnlyCollection<Blueprint>(EveYamlFactory.ParseFile<Blueprint>(Path.Combine(PathToFiles, BlueprintsFilename)));
                foreach (var blueprint in Blueprints)
                    blueprint.FillMaterials(EntityTypes);
            }
        }

        private static void TryRewriteTypesSde()
        {
            //  добавить кеширование контрольной суммы и сравнивать с ней
            var dgd2 = string.Join("", EntityTypes.Select(EveSdeModel.Serialization.EntityTypeConverter.SerializeEntityType));
            System.IO.File.WriteAllText(Path.Combine(PathToFiles, TypeIDFilename), dgd2);
        }
    }
}
