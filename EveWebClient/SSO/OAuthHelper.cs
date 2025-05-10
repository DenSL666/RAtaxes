using EveCommon.Models;
using EveCommon.Interfaces;
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

        public OAuthHelper(HttpClient httpClient, IConfig config)
        {
            ClientId = config.ClientId;
            RedirectUrl = config.CallbackUrl;
            Scopes = config.Scopes;
            HttpClient = httpClient;
        }

        #endregion


        #region Properties

        private string ClientId { get; }
        private string RedirectUrl { get; }
        private IEnumerable<string> Scopes { get; }
        private HttpClient HttpClient { get; }
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
                string body = string.Join("<br/>\n", "\t\tReturn to EveTaxes to continue calculation.", "\t\t", "\t\t<i>This browser window can now be closed.</i>");

                await WriteStringAsync(context.Response.OutputStream, myHeader, body);
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

        private Task WriteStringAsync(Stream stream, string header, string body)
        {
            string headerTemplate = "header567", bodyTemplate = "text123";

            var embededFileName = "EveWebClient.SSO.BrowserPageHtml.txt";
            var assembly = Assembly.GetExecutingAssembly();
            var embededStream = assembly.GetManifestResourceStream(embededFileName);

            if (embededStream == null)
            {
                throw new FileNotFoundException("Cannot find mappings file.", embededFileName);
            }
            string buffer;
            using (var _reader = new StreamReader(embededStream)) buffer = _reader.ReadToEnd();
            buffer = buffer
                .Replace(headerTemplate, header)
                .Replace(bodyTemplate, body);

            return Task.Run(() =>
            {
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    writer.Write(buffer);
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
