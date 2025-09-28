using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveTaxesLogic.Models
{
    /// <summary>
    /// Описывает модель налогов некоторой сущности.
    /// </summary>
    public abstract class BaseTax
    {
        /// <summary>
        /// Суммарное количество добытых Isk.
        /// </summary>
        public long TotalIskGain => TotalIskGain_MoonMining + TotalIskGain_MineralMining + TotalIskGain_Ratting;

        /// <summary>
        /// Суммарное количество налогов в Isk.
        /// </summary>
        public long TotalIskTax => TotalIskTax_MoonMining + TotalIskTax_MineralMining + TotalIskTax_Ratting;

        /// <summary>
        /// Суммарное количество добытых Isk с лун.
        /// </summary>
        public long TotalIskGain_MoonMining { get; set; }

        /// <summary>
        /// Суммарное количество налогов в Isk с лун.
        /// </summary>
        public long TotalIskTax_MoonMining { get; set; }

        /// <summary>
        /// Суммарное количество добытых Isk с минералов.
        /// </summary>
		public long TotalIskGain_MineralMining { get; set; }

        /// <summary>
        /// Суммарное количество налогов в Isk с минералов.
        /// </summary>
		public long TotalIskTax_MineralMining { get; set; }

        /// <summary>
        /// Суммарное количество добытых Isk с крабства.
        /// </summary>
		public long TotalIskGain_Ratting { get; set; }

        /// <summary>
        /// Суммарное количество налогов в Isk с крабства.
        /// </summary>
        public long TotalIskTax_Ratting { get; set; }

        /// <summary>
        /// Метод складывания всех налоговых чисел.
        /// </summary>
        public abstract void SummTaxes();
    }
}
