using EveSdeModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EveWebClient.External;
using EveWebClient.SSO;
using EveWebClient.SSO.Models;

namespace EveTaxes
{
    internal class Program
    {
        const double Effincency = 0.9063d;
        const int CorporationId = 98681778;
        static readonly string PathConfig = Path.Combine("sso", "config.xml");
        static readonly string PathAuth = Path.Combine("sso", "authData.cfg");

        private static void _Main(string[] args)
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

        private static async Task Main(string[] args)
        {
            var config = Config.Read(PathConfig);
            var helper = new OAuthHelper(config);
            var token = AccessTokenDetails.Read(PathAuth);
            

            if (!token.IsEmpty)
            {
                var isValid = await helper.IsTokenValid(token);
                if (!isValid)
                {
                    var newToken = await helper.RefreshTokenAsync(token);
                    if (newToken != null && !newToken.IsEmpty)
                    { 
                        newToken.Write(PathAuth);
                        token = newToken;
                    }
                }
            }
            else
            {
                var code = await helper.GetAuthCodeFromSSO();
                if (string.IsNullOrEmpty(code))
                    return;
                token = await helper.RequestTokenAsync(code);
                if (token != null && !token.IsEmpty)
                {
                    token.Write(PathAuth);
                }
            }

            var industry = new EsiHelper(helper);
            var answer = await industry.CorporationMiningObserversV1Async(token, CorporationId);
        }
    }
}