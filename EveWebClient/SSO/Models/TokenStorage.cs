using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveWebClient.SSO.Models
{
    public class TokenStorage
    {
        public TokenStorage()
        {
            CorporationTokens = new Dictionary<int, AccessTokenDetails>();
        }

        /// <summary>
        /// Словарь токенов для каждого id корпораций.
        /// </summary>
        [JsonProperty("corporationTokens")]
        public Dictionary<int, AccessTokenDetails> CorporationTokens { get; set; }

        /// <summary>
        /// Читает токены из файла.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <returns>Токены.</returns>
        public static TokenStorage Read(string path)
        {
            TokenStorage tokenStorage;
            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    tokenStorage = JsonConvert.DeserializeObject<TokenStorage>(reader.ReadToEnd());
                }
            }
            else
            {
                //  создаём пустое хранилище
                tokenStorage = new TokenStorage();
                tokenStorage.Write(path);
            }
            return tokenStorage;
        }

        /// <summary>
        /// Записывает токены в файл.
        /// </summary>
        /// <param name="tokenStorage">Токены.</param>
        /// <param name="path">Путь к файлу.</param>
        public static void Write(TokenStorage tokenStorage, string path)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.Write(JsonConvert.SerializeObject(tokenStorage));
            }
        }

        /// <summary>
        /// Записывает токены в файл.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        public void Write(string path) => Write(this, path);
    }
}
