using EveWebClient.SSO.Models.Esi;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace EveWebClient.SSO.Models
{
    public class EsiHelper : APIBase
    {
        public EsiHelper(OAuthHelper helper) : base(helper)
        {

        }

        /// <summary>
        /// Paginated list of all entities capable of observing and recording mining for a corporation.
        /// <para>GET /corporation/{corporation_id}/mining/observers/</para>
        /// <para>Requires one of the following EVE corporation role(s): Accountant</para>
        /// </summary>
        /// <param name="auth">The <see cref="AuthDTO"/> object.</param>
        /// <param name="corporationId">An EVE corporation ID.</param>
        /// <param name="page">Which page of results to return. Default value: 1.</param>
        /// <param name="ifNoneMatch">ETag from a previous request. A 304 will be returned if this matches the current ETag.</param>
        /// <returns><see cref="ESIModelDTO{T}"/> containing observer list of a corporation.</returns>
        public async Task<ESIModelDTO<List<CorporationMiningObserver>>> CorporationMiningObserversV1Async(AccessTokenDetails auth, int corporationId, int page = 1, string ifNoneMatch = null)
        {
            //CheckAuth(auth, Scopes.ESI_INDUSTRY_READ_CORPORATION_MINING_1);

            var queryParameters = new Dictionary<string, string>
            {
                { "page", page.ToString() }
            };

            var responseModel = await GetAsync($"/v1/corporation/{corporationId}/mining/observers/", auth, ifNoneMatch, queryParameters);

            //CheckResponse(nameof(CorporationMiningObserversV1Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<List<CorporationMiningObserver>>(responseModel);
        }

        /// <summary>
        /// Paginated record of all mining seen by an observer.
        /// <para>GET /corporation/{corporation_id}/mining/observers/{observer_id}/</para>
        /// <para>Requires one of the following EVE corporation role(s): Accountant</para>
        /// </summary>
        /// <param name="auth">The <see cref="AuthDTO"/> object.</param>
        /// <param name="corporationId">An EVE corporation ID.</param>
        /// <param name="observerId">A mining observer id.</param>
        /// <param name="page">Which page of results to return. Default value: 1.</param>
        /// <param name="ifNoneMatch">ETag from a previous request. A 304 will be returned if this matches the current ETag.</param>
        /// <returns><see cref="ESIModelDTO{T}"/> containing mining ledger of an observer.</returns>
        public async Task<ESIModelDTO<List<CorporationObservedMining>>> ObservedCorporationMiningV1Async(AccessTokenDetails auth, int corporationId, long observerId, int page = 1, string ifNoneMatch = null)
        {
            //CheckAuth(auth, Scopes.ESI_INDUSTRY_READ_CORPORATION_MINING_1);

            var queryParameters = new Dictionary<string, string>
            {
                { "page", page.ToString() }
            };

            var responseModel = await GetAsync($"/v1/corporation/{corporationId}/mining/observers/{observerId}/", auth, ifNoneMatch, queryParameters);

            //CheckResponse(nameof(ObservedCorporationMiningV1Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<List<CorporationObservedMining>>(responseModel);
        }

        /// <summary>
        /// Public information about a character.
        /// <para>GET /characters/{character_id}/</para>
        /// </summary>
        /// <param name="characterId">An EVE character ID.</param>
        /// <param name="ifNoneMatch">ETag from a previous request. A 304 will be returned if this matches the current ETag.</param>
        /// <returns><see cref="ESIModelDTO{T}"/> containing public data for the given character.</returns>
        public async Task<ESIModelDTO<CharacterInfo>> GetCharacterPublicInfoV5Async(int characterId, string ifNoneMatch = null)
        {
            var responseModel = await GetAsync($"/v5/characters/{characterId}/", ifNoneMatch);

            //CheckResponse(nameof(GetCharacterPublicInfoV5Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<CharacterInfo>(responseModel);
        }

        /// <summary>
        /// Public information about a corporation.
        /// <para>GET /corporations/{corporation_id}/</para>
        /// </summary>
        /// <param name="corporationId">An EVE corporation ID.</param>
        /// <param name="ifNoneMatch">ETag from a previous request. A 304 will be returned if this matches the current ETag.</param>
        /// <returns><see cref="ESIModelDTO{T}"/> containing public information about a corporation.</returns>
        public async Task<ESIModelDTO<CorporationInfo>> GetCorporationInfoV5Async(int corporationId, string ifNoneMatch = null)
        {
            var responseModel = await GetAsync($"/v5/corporations/{corporationId}/", ifNoneMatch);

            //CheckResponse(nameof(GetCorporationInfoV5Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<CorporationInfo>(responseModel);
        }

        /// <summary>
        /// Public information about an alliance.
        /// <para>GET /alliances/{alliance_id}/</para>
        /// </summary>
        /// <param name="allianceId">An EVE alliance ID.</param>
        /// <param name="ifNoneMatch">ETag from a previous request. A 304 will be returned if this matches the current ETag.</param>
        /// <returns><see cref="ESIModelDTO{T}"/> containing public data about an alliance.</returns>
        public async Task<ESIModelDTO<AllianceInfo>> GetAllianceInfoV3Async(int allianceId, string ifNoneMatch = null)
        {
            var responseModel = await GetAsync($"/v3/alliances/{allianceId}/", ifNoneMatch);

            //CheckResponse(nameof(GetAllianceInfoV3Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<AllianceInfo>(responseModel);
        }

        /// <summary>
        /// List all current member corporations of an alliance.
        /// <para>GET /alliances/{alliance_id}/corporations/</para>
        /// </summary>
        /// <param name="allianceId">An EVE alliance ID.</param>
        /// <param name="ifNoneMatch">ETag from a previous request. A 304 will be returned if this matches the current ETag.</param>
        /// <returns><see cref="ESIModelDTO{T}"/> containing list of corporation IDs</returns>
        public async Task<ESIModelDTO<List<int>>> ListAllianceCorporationsV1Async(int allianceId, string ifNoneMatch = null)
        {
            var responseModel = await GetAsync($"/v1/alliances/{allianceId}/corporations/", ifNoneMatch);

            //CheckResponse(nameof(ListAllianceCorporationsV1Async), responseModel.Error, responseModel.Message, responseModel.LegacyWarning, logger);

            return ReturnModelDTO<List<int>>(responseModel);
        }
    }
}
