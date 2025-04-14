using EveSdeModel.Factories;
using EveSdeModel.Interfaces;
using EveSdeModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Program
{
    static string PathToFiles { get; } = @"F:\sde\fsd";
    static string PathToFiles_2 { get; } = @"F:\sde\bsd";
    static string PathToFiles_3 { get; } = @"F:\sde\universe\eve";
    static string CategoriesFilename { get; } = @"categories.yaml";
    static string GroupsFilename { get; } = @"groups.yaml";
    static string TypeIDFilename { get; } = @"types.yaml";
    static string TypeMaterialsFilename { get; } = @"typeMaterials.yaml";
    static string BlueprintsFilename { get; } = @"blueprints.yaml";

    private static void Main(string[] args)
    {
        var typeMaterials = EveYamlFactory.ParseFile<TypeMaterial>(Path.Combine(PathToFiles, TypeMaterialsFilename));
        var categories = EveYamlFactory.ParseFile<Category>(Path.Combine(PathToFiles, CategoriesFilename));
        var groups = EveYamlFactory.ParseFile<Group>(Path.Combine(PathToFiles, GroupsFilename));
        var blueprints = EveYamlFactory.ParseFile<Blueprint>(Path.Combine(PathToFiles, BlueprintsFilename));
        var _types = EveYamlFactory.ParseFile<EntityType>(Path.Combine(PathToFiles, TypeIDFilename));

        groups.ForEach(x => x.FillCategories(categories));
        _types.ForEach(x => x.FillGroups(groups));
        blueprints.ForEach(x => x.FillMaterials(_types));

        //фильтр и вывод блюпринтов
        var hasManu = blueprints.Where(x => x.HasManufactory && !x.IsFuelBlock).ToList();
        using (var wr = new StreamWriter(@"F:\bps.txt"))
        {
            foreach (var bp in hasManu.OrderBy(x => x.Product.Name.English))
            {
                wr.WriteLine(bp.Write());
            }
        }
    }
}