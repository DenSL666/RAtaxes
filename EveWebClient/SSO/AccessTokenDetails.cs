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
    /// <summary>
    /// Токен авторизации персонажа EVE SSO.
    /// </summary>
    public class AccessTokenDetails
    {
        /// <summary>
        /// Строковое значение токена.
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Тип токена.
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// Время истечения актуальности токена в секундах.<br/>Установка значения обновляет дату истечения.
        /// </summary>
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

        /// <summary>
        /// Токен обновления токена.
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Дата истчения актуальности токена.
        /// </summary>
        [JsonProperty("expires_datetime")]
        public DateTime ExpiresUtc { get; set; }

        [JsonIgnore]
        private int _expiresIn;

        /// <summary>
        /// Проверяет, что токен корректен.
        /// </summary>
        [JsonIgnore]
        public bool IsEmpty => string.IsNullOrEmpty(AccessToken) || string.IsNullOrEmpty(RefreshToken);

        public AccessTokenDetails()
        {
            AccessToken = string.Empty;
            TokenType = string.Empty;
            ExpiresIn = 0;
            RefreshToken = string.Empty;
        }

        /// <summary>
        /// Читает токен из файла.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <returns>Токен.</returns>
        public static AccessTokenDetails Read(string path)
        {
            AccessTokenDetails tokenDetails;
            using (var reader = new StreamReader(path))
            {
                tokenDetails = JsonConvert.DeserializeObject<AccessTokenDetails>(reader.ReadToEnd());
            }
            return tokenDetails;
        }

        /// <summary>
        /// Записывает токен в файл.
        /// </summary>
        /// <param name="tokenDetails">Токен.</param>
        /// <param name="path">Путь к файлу.</param>
        public static void Write(AccessTokenDetails tokenDetails, string path)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.Write(JsonConvert.SerializeObject(tokenDetails));
            }
        }

        /// <summary>
        /// Записывает текущий токен в файл.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        public void Write(string path) => Write(this, path);
    }
}
