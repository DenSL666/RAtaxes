using EveCommon.Models;
using EveCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using Microsoft.Extensions.Configuration;

namespace EveWebClient.SSO
{
    public class AccessTokenDetails
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn
        {
            get => _expiresIn;
            set
            {
                ExpiresUtc = DateTime.UtcNow.AddSeconds(value);
                _expiresIn = value;
            }
        }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_datetime")]
        public DateTime ExpiresUtc { get; set; }

        [JsonIgnore]
        private int _expiresIn;

        [JsonIgnore]
        public bool IsEmpty => string.IsNullOrEmpty(AccessToken) || string.IsNullOrEmpty(RefreshToken);

        public AccessTokenDetails()
        {
            AccessToken = string.Empty;
            TokenType = string.Empty;
            ExpiresIn = 0;
            RefreshToken = string.Empty;
        }

        public static AccessTokenDetails Read(string path)
        {
            AccessTokenDetails tokenDetails;
            using (var reader = new StreamReader(path))
            {
                tokenDetails = JsonConvert.DeserializeObject<AccessTokenDetails>(reader.ReadToEnd());
            }
            return tokenDetails;
        }

        public static void Write(AccessTokenDetails tokenDetails, string path)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.Write(JsonConvert.SerializeObject(tokenDetails));
            }
        }

        public void Write(string path) => Write(this, path);
    }
}
