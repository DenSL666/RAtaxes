using EveDataStorage.Models;
using EveSdeModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveTaxesLogic.Models
{
    public class CorporationTax
    {
        public int CorporationId { get; }
        public string CorporationName { get; }

        public int? AllianceId { get; }
        public Alliance? Alliance { get; }

        public List<UserTax> UserTaxes { get; }

        public long TotalIskGain_MoonMining { get; set; }
        public long TotalIskTax_MoonMining { get; set; }

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

        public void SummTaxes()
        {
            TotalIskGain_MoonMining = UserTaxes.Sum(x => x.TotalIskGain_MoonMining);
            TotalIskTax_MoonMining = UserTaxes.Sum(x => x.TotalIskTax_MoonMining);
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
