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
using System.IO;

namespace EveCommon.Models
{
    /// <summary>
    /// Конфиг основного приложения, собирающего информацию для налоговой отчетности.
    /// </summary>
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
            pathConfig = Path.Combine(AppContext.BaseDirectory, pathConfig);
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

        ///<inheritdoc/>
        public void Write()
        {
            var pathConfig = _configuration.GetValue<string>("Runtime:PathConfig");
            pathConfig = Path.Combine(AppContext.BaseDirectory, pathConfig);
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            using (Stream writer = new FileStream(pathConfig, FileMode.Create))
            {
                serializer.Serialize(writer, this);
            }
        }

        /// <summary>
        /// Метод чтения конфига из файла.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <returns></returns>
        protected static Config Read(string path)
        {
            Config config;
            XmlSerializer serializer = new XmlSerializer(typeof(Config));

            /// Если файл не был найден на диске, будет считан его шаблон из встроенных ресурсов.
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

        ///<inheritdoc/>
        [XmlElement("clientId")]
        public string ClientId { get; set; }

        ///<inheritdoc/>
        [XmlElement("clientSecret")]
        public string ClientSecret { get; set; }

        ///<inheritdoc/>
        [XmlElement("callbackUrl")]
        public string CallbackUrl { get; set; }

        ///<inheritdoc/>
        [XmlArray("scopes")]
        [XmlArrayItem("scope", IsNullable = false)]
        public string[] Scopes { get; set; }

        ///<inheritdoc/>
        [XmlElement("lastUpdateDateTime")]
        public DateTime LastUpdateDateTime { get; set; }

        ///<inheritdoc/>
        [XmlElement("hoursBeforeUpdate")]
        public int HoursBeforeUpdate { get; set; }

        ///<inheritdoc/>
        [XmlElement("fuzzworkPricesUrl")]
        public string FuzzworkPricesUrl { get; set; }

        ///<inheritdoc/>
        [XmlElement("taxParams")]
        public TaxParams TaxParams { get; set; }

        ///<inheritdoc/>
        [XmlElement("seatParams")]
        public SeatParams SeatParams { get; set; }

        #endregion

        #region Constants

        /// <summary>
        /// Шаблонное значение параметра, которое нужно заменить своим.
        /// </summary>
        const string CopyYourValue = "copyYourValue";
        /// <summary>
        /// Шаблонное значение адреса сайта SEAT, которое нужно заменить своим.
        /// </summary>
        const string SeatDomain = "https://seat.temp_domain.ru";
        /// <summary>
        /// Шаблонное значение параметра api ключа для обращения к SEAT, которое нужно заменить своим.
        /// </summary>
        const string SeatApiKey = "your_seat_api_token";

        #endregion
    }

    /// <summary>
    /// Контейнер, описывающий параметры подсчёта налогов.
    /// </summary>
    public class TaxParams
    {
        /// <summary>
        /// Id корпорации, которая владеет структурами, используемыми для добычи лунной руды.
        /// </summary>
        [XmlElement("miningHoldingCorporationId")]
        public int MiningHoldingCorporationId { get; set; }

        /// <summary>
        /// Коллекция id альянсов, для которых производится расчет налогов.
        /// </summary>
        [XmlArray("allianceIdsToCalcTaxes")]
        [XmlArrayItem("allianceId", IsNullable = false)]
        public int[] AllianceIdsToCalcTaxes { get; set; }

        /// <summary>
        /// Коллекция id корпораций, которые будут исключены из итогового подсчета налогов.
        /// </summary>
        [XmlArray("corpIdsToExcludeTaxes")]
        [XmlArrayItem("corpId", IsNullable = false)]
        public int[] CorpIdsToExcludeTaxes { get; set; }

        /// <summary>
        /// Коллекция id корпораций, для которых не производится сбор данных о транзакциях корпорации.<br/>
        /// Например, инфра-корпа с 0 налогов.
        /// </summary>
        [XmlArray("corpIdsToExceptCollectWallet")]
        [XmlArrayItem("corpId", IsNullable = false)]
        public int[] CorpIdsToExceptCollectWallet { get; set; }

        /// <summary>
        /// Коллекция строковых данных, которые фильтруют общий набор транзакций корпорации.<br/>
        /// Данные относятся к формату, используемом в SEAT.
        /// </summary>
        [XmlArray("corpTransactTypes")]
        [XmlArrayItem("corpTransactType", IsNullable = false)]
        public string[] CorpTransactTypes { get; set; }

        /// <summary>
        /// Коллекция id регионов, для которых происходит подсчет налогов с накопанных руд.
        /// </summary>
        [XmlArray("mineralTaxMiningRegions")]
        [XmlArrayItem("mineralTaxMiningRegion", IsNullable = false)]
        public int[] MineralTaxMiningRegions { get; set; }

        /// <summary>
        /// Значение налога на крабство/миссии, который собирает альянс с корпораций.<br/>
        /// Записывается в формате 0.1 что значит 10% налог.
        /// </summary>
        [XmlElement("taxRatting")]
        public double TaxRatting { get; set; }

        /// <summary>
        /// Значение величины эффективности переработки руды.<br/>
        /// Записывается в формате 0.903 что значит 90.3% эффективности переработки.
        /// </summary>
        [XmlElement("refineEffincency")]
        public double RefineEffincency { get; set; }

        /// <summary>
        /// Значение налога добычу лёдных руд.<br/>
        /// Записывается в формате 0.1 что значит 10% налог.
        /// </summary>
        [XmlElement("taxIce")]
        public double TaxIce { get; set; }

        /// <summary>
        /// Значение налога добычу астероидных руд.<br/>
        /// Записывается в формате 0.1 что значит 10% налог.
        /// </summary>
        [XmlElement("taxMinerals")]
        public double TaxMinerals { get; set; }

        /// <summary>
        /// Значение налога добычу лунных R4 руд.<br/>
        /// Записывается в формате 0.1 что значит 10% налог.
        /// </summary>
        [XmlElement("taxMoonR4")]
        public double TaxMoonR4 { get; set; }

        /// <summary>
        /// Значение налога добычу лунных R8 руд.<br/>
        /// Записывается в формате 0.1 что значит 10% налог.
        /// </summary>
        [XmlElement("taxMoonR8")]
        public double TaxMoonR8 { get; set; }

        /// <summary>
        /// Значение налога добычу лунных R16 руд.<br/>
        /// Записывается в формате 0.1 что значит 10% налог.
        /// </summary>
        [XmlElement("taxMoonR16")]
        public double TaxMoonR16 { get; set; }

        /// <summary>
        /// Значение налога добычу лунных R32 руд.<br/>
        /// Записывается в формате 0.1 что значит 10% налог.
        /// </summary>
        [XmlElement("taxMoonR32")]
        public double TaxMoonR32 { get; set; }

        /// <summary>
        /// Значение налога добычу лунных R64 руд.<br/>
        /// Записывается в формате 0.1 что значит 10% налог.
        /// </summary>
        [XmlElement("taxMoonR64")]
        public double TaxMoonR64 { get; set; }

        [XmlAnyElement("RemarkComment1")]
        public XmlComment RemarkComment1 { get { return new XmlDocument().CreateComment("Allows you to choose which price to use when calculating taxes"); } set { } }
        [XmlAnyElement("RemarkComment2")]
        public XmlComment RemarkComment2 { get { return new XmlDocument().CreateComment("0 - eve average; 1 - jita sell; 2 - jita buy; 3 - jita split"); } set { } }
        
        /// <summary>
        /// Числовой переключатель, определяющий, какую цену использовать при расчете налогов на руды.<br/>
        /// Может принимать значения: 0 - среднее значение по EVE; 1 - жита селл; 2- жита бай; 3 - жита сплит.
        /// </summary>
        [XmlElement("priceSource")]
        public int PriceSource { get; set; }
    }

    /// <summary>
    /// Контейнер, описывающий параметры взаимодействия с SEAT.
    /// </summary>
    public class SeatParams
    {
        /// <summary>
        /// Url адрес сайта SEAT.<br/>
        /// SEAT используется для сопоставления имени персонажа с его основным персонажем, а так же для сбора данных о полученных корпорациями налогах с игроков.
        /// </summary>
        [XmlElement("seatUrl")]
        public string SeatUrl { get; set; }

        /// <summary>
        /// Токен/api ключ, который позволяет обращаться к endpoint'ам SEAT'а.<br/>
        /// Привязан к определённому ip-адресу клиентского приложения.
        /// </summary>
        [XmlElement("seatToken")]
        public string SeatToken { get; set; }
    }
}
