using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

using EveCommon.Interfaces;

namespace EveCommon.Models
{
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot("singleSignOn", Namespace = "", IsNullable = false)]
    public class Config : IConfig, IValidatableObject
    {
        /// <summary>
        /// Для создания из xml serializer
        /// </summary>
        internal Config()
        {
            ClientId = string.Empty;
            ClientSecret = string.Empty;
            CallbackUrl = string.Empty;
            Scopes = [];
            TaxParams = new TaxParams();
            SeatParams = new SeatParams();
        }

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Для создания Dependency Injection
        /// </summary>
        /// <param name="configuration"></param>
        public Config(IConfiguration configuration)
        {
            _configuration = configuration;
            var pathConfig = _configuration.GetValue<string>("Runtime:PathConfig");
            var config = Read(pathConfig);

            ClientId = config.ClientId;
            ClientSecret = config.ClientSecret;
            CallbackUrl = config.CallbackUrl;
            Scopes = config.Scopes;
            LastUpdateDateTime = config.LastUpdateDateTime;
            HoursBeforeUpdate = config.HoursBeforeUpdate;
            FuzzworkPricesUrl = config.FuzzworkPricesUrl;

            TaxParams = config.TaxParams;
            SeatParams = config.SeatParams;
        }

        public void Write()
        {
            var pathConfig = _configuration.GetValue<string>("Runtime:PathConfig");
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            using (Stream writer = new FileStream(pathConfig, FileMode.Create))
            {
                serializer.Serialize(writer, this);
            }
        }

        protected static Config Read(string path)
        {
            Config config;
            XmlSerializer serializer = new XmlSerializer(typeof(Config));

            if (!File.Exists(path))
            {
                if (!Directory.Exists("data"))
                    Directory.CreateDirectory("data");

                var embededFileName = "EveCommon.Models.Config.xml";
                var assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream(embededFileName);

                if (stream == null)
                {
                    throw new FileNotFoundException("Cannot find mappings file.", embededFileName);
                }
                string buffer;
                using (var _reader = new StreamReader(stream)) buffer = _reader.ReadToEnd();
                using (var writer = new StreamWriter(path)) writer.Write(buffer);
                throw new Exception($"Файл настроек приложения не был найден. Создан шаблонный файл. Внесите в него необходимые данные (\"{path}\")");
            }

            try
            {
                using (Stream reader = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    // Call the Deserialize method to restore the object's state.
                    config = (Config)serializer.Deserialize(reader);
                }
            }
            catch { throw; }

            var context = new ValidationContext(config);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(config, context, results, true))
            {
                var message = "При проверке файла настроек приложения были найдены следующие ошибки:";
                var errors = results.Select(x => x.ErrorMessage).ToList();
                errors.Insert(0, message);
                message = string.Join(Environment.NewLine, errors);
                throw new Exception(message);
            }

            return config;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();

            if (string.IsNullOrEmpty(ClientId) || ClientId == CopyYourValue)
                errors.Add(new ValidationResult($"Значение ClientId не указано или некорректно ({CopyYourValue})"));

            if (string.IsNullOrEmpty(CallbackUrl) || CallbackUrl == CopyYourValue)
                errors.Add(new ValidationResult($"Значение CallbackUrl не указано или некорректно ({CopyYourValue})"));

            if (Scopes.Length == 0)
                errors.Add(new ValidationResult("Не указаны Scopes"));
            else if (Scopes.Any(x => x == CopyYourValue))
                errors.Add(new ValidationResult($"Одно из значений Scopes некорректно ({CopyYourValue})"));

            if (string.IsNullOrEmpty(FuzzworkPricesUrl))
                errors.Add(new ValidationResult($"Значение FuzzworkPricesUrl не указано"));

            if (SeatParams != null)
            {
                if (string.IsNullOrEmpty(SeatParams.SeatToken) || SeatParams.SeatToken == SeatApiKey)
                    errors.Add(new ValidationResult($"Значение SeatToken не указано или некорректно ({SeatApiKey})"));

                if (string.IsNullOrEmpty(SeatParams.SeatUrl) || SeatParams.SeatUrl == SeatDomain)
                    errors.Add(new ValidationResult($"Значение SeatUrl не указано или некорректно ({SeatDomain})"));
            }

            return errors;
        }

        #region Properties

        [XmlElement("clientId")]
        public string ClientId { get; set; }

        [XmlElement("clientSecret")]
        public string ClientSecret { get; set; }

        [XmlElement("callbackUrl")]
        public string CallbackUrl { get; set; }

        [XmlArray("scopes")]
        [XmlArrayItem("scope", IsNullable = false)]
        public string[] Scopes { get; set; }


        [XmlElement("lastUpdateDateTime")]
        public DateTime LastUpdateDateTime { get; set; }

        [XmlElement("hoursBeforeUpdate")]
        public int HoursBeforeUpdate { get; set; }

        [XmlElement("fuzzworkPricesUrl")]
        public string FuzzworkPricesUrl { get; set; }

        [XmlElement("taxParams")]
        public TaxParams TaxParams { get; set; }

        [XmlElement("seatParams")]
        public SeatParams SeatParams { get; set; }

        #endregion

        #region Constants

        const string CopyYourValue = "copyYourValue";
        const string SeatDomain = "https://seat.temp_domain.ru";
        const string SeatApiKey = "your_seat_api_token";

        #endregion
    }

    public class TaxParams
    {
        [XmlElement("miningHoldingCorporationId")]
        public int MiningHoldingCorporationId { get; set; }

        [XmlArray("allianceIdsToCalcTaxes")]
        [XmlArrayItem("allianceId", IsNullable = false)]
        public int[] AllianceIdsToCalcTaxes { get; set; }

        [XmlArray("corpIdsToExceptCollectWallet")]
        [XmlArrayItem("corpId", IsNullable = false)]
        public int[] CorpIdsToExceptCollectWallet { get; set; }

        [XmlElement("refineEffincency")]
        public double RefineEffincency { get; set; }
        [XmlElement("taxIce")]
        public double TaxIce { get; set; }
        [XmlElement("taxMinerals")]
        public double TaxMinerals { get; set; }
        [XmlElement("taxMoonR4")]
        public double TaxMoonR4 { get; set; }
        [XmlElement("taxMoonR8")]
        public double TaxMoonR8 { get; set; }
        [XmlElement("taxMoonR16")]
        public double TaxMoonR16 { get; set; }
        [XmlElement("taxMoonR32")]
        public double TaxMoonR32 { get; set; }
        [XmlElement("taxMoonR64")]
        public double TaxMoonR64 { get; set; }

        [XmlAnyElement("RemarkComment1")]
        public XmlComment RemarkComment1 { get { return new XmlDocument().CreateComment("Allows you to choose which price to use when calculating taxes"); } set { } }
        [XmlAnyElement("RemarkComment2")]
        public XmlComment RemarkComment2 { get { return new XmlDocument().CreateComment("0 - eve average; 1 - jita sell; 2 - jita buy; 3 - jita split"); } set { } }
        [XmlElement("priceSource")]
        public int PriceSource { get; set; }
    }

    public class SeatParams
    {
        [XmlElement("seatUrl")]
        public string SeatUrl { get; set; }

        [XmlElement("seatToken")]
        public string SeatToken { get; set; }
    }
}
