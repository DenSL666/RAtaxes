using EveCommon.Interfaces;
using EveWebClient.Esi.Models;
using EveWebClient.SSO;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EveWebClient.Esi
{
    public class EsiHelper : APIBase
    {
        public EsiHelper(HttpClient httpClient, IConfig config) : base(httpClient, config)
        {

        }

        public async Task<ESIModelDTO<List<CorporationMiningObserver>>> CorporationMiningObserversV1Async(AccessTokenDetails auth, int corporationId, int page = 1, string ifNoneMatch = null)
        {
            CheckAuth(auth, Scopes.ESI_INDUSTRY_READ_CORPORATION_MINING_1);

            var queryParameters = new Dictionary<string, string>
            {
                { "page", page.ToString() }
            };

            var responseModel = await GetAsync($"/v1/corporation/{corporationId}/mining/observers/", auth, ifNoneMatch, queryParameters);

            //CheckResponse(nameof(CorporationMiningObserversV1Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<List<CorporationMiningObserver>>(responseModel);
        }

        public async Task<ESIModelDTO<List<CorporationObservedMining>>> ObservedCorporationMiningV1Async(AccessTokenDetails auth, int corporationId, long observerId, int page = 1, string ifNoneMatch = null)
        {
            CheckAuth(auth, Scopes.ESI_INDUSTRY_READ_CORPORATION_MINING_1);

            var queryParameters = new Dictionary<string, string>
            {
                { "page", page.ToString() }
            };

            var responseModel = await GetAsync($"/v1/corporation/{corporationId}/mining/observers/{observerId}/", auth, ifNoneMatch, queryParameters);

            //CheckResponse(nameof(ObservedCorporationMiningV1Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<List<CorporationObservedMining>>(responseModel);
        }

        public async Task<ESIModelDTO<CharacterInfo>> GetCharacterPublicInfoV5Async(int characterId, string ifNoneMatch = null)
        {
            var responseModel = await GetAsync($"/v5/characters/{characterId}/", ifNoneMatch);

            //CheckResponse(nameof(GetCharacterPublicInfoV5Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<CharacterInfo>(responseModel, characterId);
        }

        public async Task<ESIModelDTO<CorporationInfo>> GetCorporationInfoV5Async(int corporationId, string ifNoneMatch = null)
        {
            var responseModel = await GetAsync($"/v5/corporations/{corporationId}/", ifNoneMatch);

            //CheckResponse(nameof(GetCorporationInfoV5Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<CorporationInfo>(responseModel, corporationId);
        }

        public async Task<ESIModelDTO<AllianceInfo>> GetAllianceInfoV3Async(int allianceId, string ifNoneMatch = null)
        {
            var responseModel = await GetAsync($"/v3/alliances/{allianceId}/", ifNoneMatch);

            //CheckResponse(nameof(GetAllianceInfoV3Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<AllianceInfo>(responseModel, allianceId);
        }

        public async Task<ESIModelDTO<List<int>>> ListAllianceCorporationsV1Async(int allianceId, string ifNoneMatch = null)
        {
            var responseModel = await GetAsync($"/v1/alliances/{allianceId}/corporations/", ifNoneMatch);

            //CheckResponse(nameof(ListAllianceCorporationsV1Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<List<int>>(responseModel, allianceId);
        }

        public async Task<ESIModelDTO<List<CorporationStructure>>> GetCorporationStructuresV3Async(AccessTokenDetails auth, int corporationId, int page = 1, string language = "en-us", string ifNoneMatch = null)
        {
            CheckAuth(auth, Scopes.ESI_CORPORATIONS_READ_STRUCTURES_1);

            var queryParameters = new Dictionary<string, string>
            {
                { "page", page.ToString() },
                { "language", language }
            };
            //  /v4/corporations/{corporation_id}/structures/
            var responseModel = await GetAsync($"/v3/corporations/{corporationId}/structures/", auth, ifNoneMatch, queryParameters);

            //CheckResponse(nameof(GetCorporationStructuresV3Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<List<CorporationStructure>>(responseModel);
        }

        public async Task<ESIModelDTO<List<MarketPrice>>> ListMarketPricesV1Async(string ifNoneMatch = null)
        {
            var responseModel = await GetAsync("/v1/markets/prices/", ifNoneMatch);

            //CheckResponse(nameof(ListMarketPricesV1Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<List<MarketPrice>>(responseModel);
        }
    }
}
