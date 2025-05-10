using EveCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EveCommon.Interfaces
{
    public interface IConfig
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string CallbackUrl { get; set; }

        public string[] Scopes { get; set; }

        public DateTime LastUpdateDateTime { get; set; }

        public int HoursBeforeUpdate { get; set; }

        public string FuzzworkPricesUrl { get; set; }

        public TaxParams TaxParams { get; set; }

        public SeatParams SeatParams { get; set; }

        public void Write();
    }
}
