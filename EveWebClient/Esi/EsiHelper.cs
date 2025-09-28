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
    /// <summary>
    /// Содержит методы получения данных от EVE Esi сервера.
    /// </summary>
    public class EsiHelper : APIBase
    {
        /// <inheritdoc/>
        public EsiHelper(HttpClient httpClient, IConfig config, ILogger<APIBase> logger) : base(httpClient, config, logger)
        {

        }

        /// <summary>
        /// Получает список структур корпорации, у которых было выкопана лунная руда, с датой сбора руды.<br/>
        /// Mining Ledger корпорации.
        /// </summary>
        /// <param name="auth">Токен авторизации пользователя с правами чтения Mining Ledger корпорации.</param>
        /// <param name="corporationId">Id корпорации.</param>
        /// <param name="page">Номер страницы в данных EVE Esi.</param>
        /// <param name="ifNoneMatch"></param>
        /// <returns>Список структур корпорации.</returns>
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

        /// <summary>
        /// Получает mining ledger выбранной структуры корпорации (за некоторый период сбора руды).
        /// </summary>
        /// <param name="auth">Токен авторизации пользователя с правами чтения Mining Ledger корпорации.</param>
        /// <param name="corporationId">Id корпорации.</param>
        /// <param name="observerId">Id леджера структуры за некоторый период сбора руды.</param>
        /// <param name="page">Номер страницы в данных EVE Esi.</param>
        /// <param name="ifNoneMatch"></param>
        /// <returns>Список того, кто что и сколько добыл.</returns>
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

        /// <summary>
        /// Получает публичные данные персонажа.
        /// </summary>
        /// <param name="characterId">Id персонажа.</param>
        /// <param name="ifNoneMatch"></param>
        /// <returns>Данные о персонаже.</returns>
        public async Task<ESIModelDTO<CharacterInfo>> GetCharacterPublicInfoV5Async(int characterId, string ifNoneMatch = null)
        {
            var responseModel = await GetAsync($"/v5/characters/{characterId}/", ifNoneMatch);

            //CheckResponse(nameof(GetCharacterPublicInfoV5Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<CharacterInfo>(responseModel, characterId);
        }

        /// <summary>
        /// Получает публичные данные корпорации.
        /// </summary>
        /// <param name="corporationId">Id корпорации.</param>
        /// <param name="ifNoneMatch"></param>
        /// <returns>Данные о корпорации.</returns>
        public async Task<ESIModelDTO<CorporationInfo>> GetCorporationInfoV5Async(int corporationId, string ifNoneMatch = null)
        {
            var responseModel = await GetAsync($"/v5/corporations/{corporationId}/", ifNoneMatch);

            //CheckResponse(nameof(GetCorporationInfoV5Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<CorporationInfo>(responseModel, corporationId);
        }

        /// <summary>
        /// Получает публичные данные альянса.
        /// </summary>
        /// <param name="allianceId">Id альянса.</param>
        /// <param name="ifNoneMatch"></param>
        /// <returns>Данные о альянса.</returns>
        public async Task<ESIModelDTO<AllianceInfo>> GetAllianceInfoV3Async(int allianceId, string ifNoneMatch = null)
        {
            var responseModel = await GetAsync($"/v3/alliances/{allianceId}/", ifNoneMatch);

            //CheckResponse(nameof(GetAllianceInfoV3Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<AllianceInfo>(responseModel, allianceId);
        }

        /// <summary>
        /// Получает список id корпораций в составе альянса.
        /// </summary>
        /// <param name="allianceId">Id альянса.</param>
        /// <param name="ifNoneMatch"></param>
        /// <returns>Список id корпораций.</returns>
        public async Task<ESIModelDTO<List<int>>> ListAllianceCorporationsV1Async(int allianceId, string ifNoneMatch = null)
        {
            var responseModel = await GetAsync($"/v1/alliances/{allianceId}/corporations/", ifNoneMatch);

            //CheckResponse(nameof(ListAllianceCorporationsV1Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<List<int>>(responseModel, allianceId);
        }

        /// <summary>
        /// Получает список структур корпорации.
        /// </summary>
        /// <param name="auth">Токен авторизации пользователя с правами чтения структур корпорации.</param>
        /// <param name="corporationId">Id корпорации.</param>
        /// <param name="page">Номер страницы в данных EVE Esi.</param>
        /// <param name="language"></param>
        /// <param name="ifNoneMatch"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Получает список цен на все предметы EVE.
        /// </summary>
        /// <param name="ifNoneMatch"></param>
        /// <returns></returns>
        public async Task<ESIModelDTO<List<MarketPrice>>> ListMarketPricesV1Async(string ifNoneMatch = null)
        {
            var responseModel = await GetAsync("/v1/markets/prices/", ifNoneMatch);

            //CheckResponse(nameof(ListMarketPricesV1Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<List<MarketPrice>>(responseModel);
        }
    }
}
