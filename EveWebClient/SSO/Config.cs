using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace EveWebClient.SSO
{
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot("singleSignOn", Namespace = "", IsNullable = false)]
    public class Config
    {
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

        [XmlElement("taxParams")]
        public TaxParams TaxParams { get; set;}

        internal Config()
        {
            ClientId = string.Empty;
            ClientSecret = string.Empty;
            CallbackUrl = string.Empty;
            Scopes = new string[0];
            TaxParams = new TaxParams();
        }

        public static Config Read(string path)
        {
            Config config;
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            using (Stream reader = new FileStream(path, FileMode.Open))
            {
                // Call the Deserialize method to restore the object's state.
                config = (Config)serializer.Deserialize(reader);
            }
            return config;
        }

        public void Write(string path) => Write(this, path);
        public static void Write(Config config, string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            using (Stream writer = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(writer, config);
            }
        }
    }

    public class TaxParams
    {
        [XmlElement("miningHoldingCorporationId")]
        public string MiningHoldingCorporationId { get; set; }
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

}
