using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveTaxesLogic.Models
{
    public abstract class BaseTax
    {
        public long TotalIskGain => TotalIskGain_MoonMining + TotalIskGain_Ratting;
        public long TotalIskTax => TotalIskTax_MoonMining + TotalIskTax_Ratting;

        public long TotalIskGain_MoonMining { get; set; }
        public long TotalIskTax_MoonMining { get; set; }

        public long TotalIskGain_Ratting { get; set; }
        public long TotalIskTax_Ratting { get; set; }

        public abstract void SummTaxes();
    }
}
