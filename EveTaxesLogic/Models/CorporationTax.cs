using EveDataStorage.Models;
using EveSdeModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveTaxesLogic.Models
{
    /// <summary>
    /// Описывает модель налогов одной корпорации со всеми её пользователями.
    /// </summary>
    public class CorporationTax : BaseTax
    {
        /// <summary>
        /// Id корпорации.
        /// </summary>
        public int CorporationId { get; }

        /// <summary>
        /// Имя корпорации.
        /// </summary>
        public string CorporationName { get; }

        /// <summary>
        /// Id альянса.
        /// </summary>
        public int? AllianceId { get; }

        /// <summary>
        /// Сущность альянса.
        /// </summary>
        public Alliance? Alliance { get; }

        /// <summary>
        /// Коллекция налогов пользователей корпорации.
        /// </summary>
        public List<UserTax> UserTaxes { get; }

        protected CorporationTax()
        {
            UserTaxes = new List<UserTax>();
        }

        public CorporationTax(int corporationId, string corporationName) : this()
        {
            CorporationId = corporationId;
            CorporationName = corporationName;
        }

        public CorporationTax(Corporation corporation) : this()
        {
            CorporationId = corporation.CorporationId;
            CorporationName = corporation.Name;

            if (corporation.AllianceId.HasValue)
                AllianceId = corporation.AllianceId.Value;
            if (corporation.Alliance != null)
                Alliance = corporation.Alliance;
        }

        public override void SummTaxes()
        {
            TotalIskGain_MoonMining = UserTaxes.Sum(x => x.TotalIskGain_MoonMining);
            TotalIskTax_MoonMining = UserTaxes.Sum(x => x.TotalIskTax_MoonMining);

			TotalIskGain_MineralMining = UserTaxes.Sum(x => x.TotalIskGain_MineralMining);
			TotalIskTax_MineralMining = UserTaxes.Sum(x => x.TotalIskTax_MineralMining);

			TotalIskGain_Ratting = UserTaxes.Sum(x => x.TotalIskGain_Ratting);
            TotalIskTax_Ratting = UserTaxes.Sum(x => x.TotalIskTax_Ratting);
        }

        public override string ToString()
        {
            var text = CorporationName;
            if (AllianceId.HasValue && Alliance != null)
                text = $"[{Alliance.Name}] {CorporationName}";
            return text;
        }
    }
}
