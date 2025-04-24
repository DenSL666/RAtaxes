using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using static System.Formats.Asn1.AsnWriter;

namespace EveWebClient.SSO
{
    public class OAuthHelper
    {
        #region Construct

        public OAuthHelper(string clientId, string redirectUrl, IEnumerable<string> scopes)
        {
            ClientId = clientId;
            RedirectUrl = redirectUrl;
            Scopes = scopes;

            HttpClient = new HttpClient();
        }

        public OAuthHelper(Config config) : this(config.ClientId, config.CallbackUrl, config.Scopes)
        {

        }

        #endregion


        #region Properties

        private string ClientId { get; }
        private string RedirectUrl { get; }
        private IEnumerable<string> Scopes { get; }
        internal HttpClient HttpClient { get; }
        private string CodeVerifier { get; set; }
        private string CodeChallenge { get; set; }

        #endregion Constants

        private const string ExpectedAudience = "EVE Online";
        private const string TRANQUILITY_SSO_BASE_URL = "https://login.eveonline.com/v2";
        private const string SERENITY_SSO_BASE_URL = "https://login.evepc.163.com/v2";
        private const string SSO_AUTHORIZE = "/oauth/authorize";
        private const string SSO_TOKEN = "/oauth/token";
        private const string SSO_REVOKE = "/oauth/revoke";
        private const string SSO_META_DATA_URL = "https://login.eveonline.com/.well-known/oauth-authorization-server";
        private const int MetadataCacheTime = 300; // 5 minutes in seconds
        private static readonly string[] AcceptedIssuers = { "logineveonline.com", "https://login.eveonline.com" };

        private static string AuthUrl => TRANQUILITY_SSO_BASE_URL + SSO_AUTHORIZE;
        private static string TokenUrl => TRANQUILITY_SSO_BASE_URL + SSO_TOKEN;
        private static string RevokeUrl => TRANQUILITY_SSO_BASE_URL + SSO_REVOKE;

        #region Static properties

        private static readonly Random random = new Random();

        private static JwksMetadata _jwksMetadata;
        private static DateTime _jwksMetadataExpiry = DateTime.MinValue;
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly object _lock = new object();

        #endregion


        #region private helper methods

        private void GenerateCodeChallenge()
        {
            // Generate code verifier
            byte[] tokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            CodeVerifier = Base64UrlEncode(tokenBytes);

            // Generate code challenge
            using (var sha256 = SHA256.Create())
            {
                byte[] challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(CodeVerifier));
                CodeChallenge = Base64UrlEncode(challengeBytes).TrimEnd('=');
            }
        }

        private (string url, string state) CreateSsoUrl()
        {
            string state = GenerateRandomString(16);
            var queryParams = new Dictionary<string, string>
            {
                { "response_type", "code" },
                { "redirect_uri", RedirectUrl },
                { "client_id", ClientId },
                { "scope", string.Join(" ", Scopes) },
                { "code_challenge", CodeChallenge },
                { "code_challenge_method", "S256" },
                { "state", state },
            };

            //var fsdfs = queryParams["scope"];
            //var sdfd = HttpUtility.UrlEncode(fsdfs);
            //var sdfd23 = Uri.EscapeDataString(fsdfs);

            string queryString = AuthUrl + "/?" + string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            return (queryString, state);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion

        #region Auth public

        public async Task<AccessTokenDetails> RefreshTokenAsync(AccessTokenDetails tokenDetails) => await RefreshTokenAsync(tokenDetails.RefreshToken);

        public async Task<AccessTokenDetails> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", refreshToken },
                    { "client_id", ClientId },
                };

                //if (Scopes != null && Scopes.Any())
                //{
                //    payload.Add("scope", Uri.EscapeDataString(string.Join(" ", Scopes)));
                //}

                //if (Scopes != null && Scopes.Any())
                //{
                //    payload.Add("scope", HttpUtility.UrlEncode(String.Join(" ", Scopes)));
                //}

                var content = new FormUrlEncodedContent(payload);

                var response = await HttpClient.PostAsync(TokenUrl, content);
                response.EnsureSuccessStatusCode();

                var token = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AccessTokenDetails>(token);
                return result;
            }
            catch (Exception exc)
            {

                throw;
            }
        }

        public async Task<AccessTokenDetails> RequestTokenAsync(string authorizationCode)
        {
            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/x-www-form-urlencoded" }
            };

            var payload = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", authorizationCode },
                { "client_id", ClientId },
                { "code_verifier", CodeVerifier }
            };

            var content = new FormUrlEncodedContent(payload);

            var response = await HttpClient.PostAsync(TokenUrl, content);
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AccessTokenDetails>(responseString);
        }

        public async Task<string> GetAuthCodeFromSSO()
        {
            string responseString = "";
            HttpListener HttpListener = null;
            GenerateCodeChallenge();

            var (url, state) = CreateSsoUrl();
            try
            {
                HttpListener = new HttpListener();

                var _redirectUrl = RedirectUrl;
                if (!_redirectUrl.EndsWith("/"))
                    _redirectUrl += "/";
                HttpListener.Prefixes.Add(_redirectUrl);
                HttpListener.Start();

                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true,
                });

                var context = await HttpListener.GetContextAsync();

                responseString = context.Request.Url.ToString();// await response.Content.ReadAsStringAsync();

                string myHeader = "Authentication Successful";
                string myResponse = "		Return to jEveAssets to complete the import.<br />\n"
                                + "		<br />\n"
                                + "		<i>This browser window can now be closed.</i>\n";

                await WriteStringAsync(context.Response.OutputStream, myHeader, myResponse);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                HttpListener?.Stop();
                HttpListener?.Close();
            }

            if (!string.IsNullOrEmpty(responseString))
            {
                var answer = responseString.Replace(RedirectUrl, "").Trim('/').Trim('?');
                var _params = answer.Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]);
                if (_params.ContainsKey("code") && _params.ContainsKey("state") && _params["state"] == state)
                {
                    return _params["code"];
                }

            }
            return "";
        }

        #endregion

        #region JWT

        internal async Task<Dictionary<string, string>> ValidateJwtToken(string token)
        {
            var metadata = await FetchJwksMetadataAsync();
            var keys = metadata.keys;

            // Get the unverified header to find the key ID and algorithm
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var header = jwtToken.Header;

            // Find the matching key
            var key = keys.FirstOrDefault(k =>
                k.kid == header.Kid &&
                k.alg == header.Alg);

            if (key == null)
            {
                throw new SecurityTokenException("No matching key found for the token");
            }

            // Configure token validation parameters
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = AcceptedIssuers,
                ValidateAudience = true,
                ValidAudience = ExpectedAudience,
                IssuerSigningKey = key.SecurityKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
            };

            try
            {
                // Validate the token
                var principal = handler.ValidateToken(token, validationParameters, out _);

                // Convert claims to dictionary for similar output to Python version

                var dict = principal.Claims
                    .GroupBy(x => x.Type)
                    .ToDictionary(x => x.Key, x => string.Join(' ', x.Select(y => y.ToString())));

                return dict;
            }
            catch (SecurityTokenExpiredException)
            {
                throw; // Rethrow as this is handled separately in IsTokenValid
            }
            catch (SecurityTokenException ex)
            {
                throw new SecurityTokenException("Invalid JWT token", ex);
            }
        }

        public async Task<bool> IsTokenValid(AccessTokenDetails tokenDetails) => await IsTokenValid(tokenDetails.AccessToken);

        public async Task<bool> IsTokenValid(string token)
        {
            try
            {
                var claims = await ValidateJwtToken(token); // Note: In real code, prefer async all the way
                var audiences = claims.Where(c => c.Key == "aud").Select(c => c.Value).ToList();

                // If our client_id is in the audience list, the token is valid
                var contains = audiences.Any(x => x.Contains(ClientId));
                return contains;
            }
            catch (SecurityTokenExpiredException)
            {
                // The token has expired
                return false;
            }
            catch (SecurityTokenException)
            {
                // The token is invalid
                return false;
            }
            catch (Exception exc)
            {
                // Something went wrong
                return false;
            }
        }

        private async Task<JwksMetadata> FetchJwksMetadataAsync()
        {
            lock (_lock)
            {
                if (_jwksMetadata != null && DateTime.UtcNow < _jwksMetadataExpiry)
                {
                    return _jwksMetadata;
                }
            }

            // Fetch metadata
            var metadataResponse = await _httpClient.GetAsync(SSO_META_DATA_URL);
            metadataResponse.EnsureSuccessStatusCode();

            var resp = await metadataResponse.Content.ReadAsStringAsync();
            var metadata = JsonConvert.DeserializeObject<OAuthMetadata>(resp);

            // Fetch JWKS data
            var jwksResponse = await _httpClient.GetAsync(metadata.jwks_uri);
            jwksResponse.EnsureSuccessStatusCode();

            var resp2 = await jwksResponse.Content.ReadAsStringAsync();
            var jwksData = JsonConvert.DeserializeObject<JwksMetadata>(resp2);

            lock (_lock)
            {
                _jwksMetadata = jwksData;
                _jwksMetadataExpiry = DateTime.UtcNow.AddSeconds(MetadataCacheTime);
                return _jwksMetadata;
            }
        }

        #endregion

        private Task WriteStringAsync(Stream stream, string header, string text)
        {
            var str = "<!DOCTYPE html>\n"
                + "<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">\n"
            + "<head>\n"
                + "	<title>" + header + "</title>\n"
                + "	<link id=\"favicon\" rel=\"shortcut icon\" type=\"image/png\" href=\" data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAACf0lEQVQ4jY2Ty08TYRTFz/2+mWk7Q2lpyktAHgFCjGh8xbhk4YLEtTujLl36T0g0JiYm/gmuXbhzgSYuxAhEMCYYRFBehWKbTtt5fvNdV1USUDnbm/vLuTfnAP8X/Wsoj90gIikNQ0opAfCJ6UIIYZiW1TUwOH724uW7hhSdbr2+VN4tfTwoba+6lZ/7cRgEWmt9BJC2HWfs3KWpkeGhe4VMNOWkOWPbKUSRQqSYvQAuzPzmyuq35+9fv3rIzAwABgAIKeX0zVtPJ8cGbxvkS0pcpAxGLt8OyzIRhoqyxfHc/OJqznFK1w67NgAAzJwkSvtBLIvdp9HT24dc1sRIbxamoRCoLGYezGBpfg6ZYn/jCICZ2TbERGlzDV+W5zA6MYnJC1exlXJgSgtPZu6jerAP2zahwVUhhJUkSQSAZevrmUKXFdb2b3R35qCCBrbWPqNarcGPgE8LbyAEADDqXjxfKZdXmDkkIiVaDgLP25RWG3oGz4CkifZCEW6tCrfmghMNBmA7FrPWXUR0HsAwM2eN1i21Snmn4KSxuLCIOIrQltlG38Awyvt7COMYliVgZ1JaSu7WWncDqACoihYg9H03CgPPMAyo0EUmbaJWc7Gz/QNxHEMQkGvPa51oArMPYAdA7TcArNE/Mnawt72OjrwN3wvQbDRwUC5DxQpKKbB0gq2N9Q/M/BbAdyJqyj8OvKZI2dTX23klCryM7wcACbiNEEGzglxHj56dffey2ag/IqINAAEAPhJlK5XuOTU0cndwoPeOkzZHv66XkM+KZHlp5ZnXbDwmol1mVsd24RBICinb03bbtBTiut9szCoVv2DmZivCJxURkSCiv1b6F/mJMFVr2FOjAAAAAElFTkSuQmCC\" />\n"
                + "	<style>\n"
                + "		body{\n"
                + "			background-color: #333;\n"
                + "			color: #c5c5c4;\n"
                + "			font-family: Arial, Helvetica, sans-serif;\n"
                + "			font-size: 11pt;\n"
                + "			margin: 0px;\n"
                + "			padding: 0px;\n"
                + "		}\n"
                + "		#menu{\n"
                + "			background-color: #111;\n"
                + "			padding: 5px 7px 5px 7px;\n"
                + "		}\n"
                + "		#headline{\n"
                + "			border: solid #000;\n"
                + "			border-width: 1px 0 1px 0;\n"
                + "			background-color: #181818;\n"
                + "			font-size: 22px;\n"
                + "			padding: 3px 7px 3px 7px;\n"
                + "			display: flex;\n"
                + "			align-items: center;\n"
                + "		}\n"
                + "		#breadcrumb{\n"
                + "			background-color: #222;\n"
                + "			border: solid #000;\n"
                + "			border-width: 0 0 1px 0;\n"
                + "			padding: 5px 7px 5px 7px;\n"
                + "		}\n"
                + "		#text{\n"
                + "			padding: 40px;\n"
                + "			margin: auto;\n"
                + "		}\n"
                + "		a {\n"
                + "			color: #ffa600;\n"
                + "		}\n"
                + "		a:hover {\n"
                + "			background-color: #8e610a;\n"
                + "		}\n"
                + "		a:visited {\n"
                + "			color: #ffcc00;\n"
                + "		}"
                + "	</style>\n"
                + "</head>\n"
                + "<body>\n"
                + "	<div id=\"menu\"></div>\n"
            + "	<div id=\"headline\">\n"
                + "		<img src=\"data:image/png;base64,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\" />" + header + "\n"
                + "	</div>\n"
                + "	<div id=\"breadcrumb\"></div>\n"
                + "	<div id=\"text\">\n"
                + text + "<br />\n"
                + "	</div>\n"
                + "</body>\n"
                + "</html>";
            return Task.Run(() =>
            {
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    writer.Write(str);
                }
            });
        }

        #region private classes

        internal class OAuthMetadata
        {
            public string issuer { get; set; }
            public string authorization_endpoint { get; set; }
            public string token_endpoint { get; set; }
            public string[] response_types_supported { get; set; }

            public string jwks_uri { get; set; }

            public string revocation_endpoint { get; set; }
            public string[] subject_types_supported { get; set; }
            public string[] revocation_endpoint_auth_methods_supported { get; set; }
            public string[] token_endpoint_auth_methods_supported { get; set; }
            public string[] id_token_signing_alg_values_supported { get; set; }
            public string[] token_endpoint_auth_signing_alg_values_supported { get; set; }
            public string[] code_challenge_methods_supported { get; set; }
        }

        internal class JwksMetadata
        {
            public Key[] keys { get; set; }
            public bool SkipUnresolvedJsonWebKeys { get; set; }
        }

        internal class Key
        {
            public string alg { get; set; }
            public string e { get; set; }
            public string kid { get; set; }
            public string kty { get; set; }
            public string n { get; set; }
            public string use { get; set; }
            public string crv { get; set; }
            public string x { get; set; }
            public string y { get; set; }

            public SecurityKey SecurityKey
            {
                get
                {
                    if (kty == "RSA")
                    {
                        var rsa = RSA.Create();
                        rsa.ImportParameters(new RSAParameters
                        {
                            Modulus = Base64UrlEncoder.DecodeBytes(n),
                            Exponent = Base64UrlEncoder.DecodeBytes(e)
                        });
                        return new RsaSecurityKey(rsa);
                    }
                    throw new NotSupportedException($"Key type {kty} is not supported");
                }
            }
        }

        #endregion
    }
}
