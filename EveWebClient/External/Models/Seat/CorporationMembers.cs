using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveWebClient.External.Models.Seat
{
    public class CorporationMembers
    {
        [JsonProperty("data")]
        public CharacterInfo[] CharacterInfoArray { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    public class CharacterInfo
    {
        public int character_id { get; set; }
        public string start_date { get; set; }
        public object base_id { get; set; }
        public string logon_date { get; set; }
        public string logoff_date { get; set; }
        public long location_id { get; set; }
    }


}
