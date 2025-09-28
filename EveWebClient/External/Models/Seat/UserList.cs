using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveWebClient.External.Models.Seat
{
    /// <summary>
    /// Обёртка для ответа сеата на запрос списка аккаунтов.
    /// </summary>
    public class UserList
    {
        [JsonProperty("data")]
        public UserInfo[] UserInfoArray { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }

    }

    /// <summary>
    /// Данные об аккаунте сеата.
    /// </summary>
    public class UserInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        //public string email { get; set; }
        public bool active { get; set; }
        //public DateTime last_login { get; set; }
        //public string last_login_source { get; set; }
        /// <summary>
        /// Список id связанных с аккаунтом персонажей.
        /// </summary>
        public int[] associated_character_ids { get; set; }
        /// <summary>
        /// Id главного персонажа аккаунта.
        /// </summary>
        public int main_character_id { get; set; }
    }

}
