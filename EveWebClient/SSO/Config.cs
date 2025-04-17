using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        internal Config()
        {
            ClientId = string.Empty;
            ClientSecret = string.Empty;
            CallbackUrl = string.Empty;
            Scopes = new string[0];
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
    }
}
