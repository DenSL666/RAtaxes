using EveSdeModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EveWebClient;

namespace EveTaxes
{
    internal class Program
    {
        const double Effincency = 0.9063d;

        private static void Main(string[] args)
        {
            SdeMain.InitializeAll(false);
            var asteroid = SdeMain.TypeMaterials.Where(x => x.IsAsteroid).ToList();
            var refineItems = asteroid.SelectMany(x => x.RefineMaterials.Keys).GroupBy(x => x.Id).Select(x => x.First()).ToList();
            var refineIds = refineItems.Select(x => x.Id).ToList();

            var found = asteroid.FirstOrDefault(x => x.ToString() == "Monazite");
            if (found != null)
            {
                var refined = found.Refine("1000", Effincency);
            }

            var prices = Price.GetPrices(refineIds);
        }
    }
}