using EveCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EveCommon.Interfaces
{
    /// <summary>
    /// Конфиг основного приложения, собирающего информацию для налоговой отчетности.
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Id клиентского приложения, используемого для авторизации EVE SSO.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Секретная часть токена клиентского приложения, используемого для авторизации EVE SSO.<br/>
        /// Не требуется для использоания приложения из-за проблем с безопасностью его хранения.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Возвратная ссылка, указанная для клиентского приложения, используемого для авторизации EVE SSO.
        /// </summary>
        public string CallbackUrl { get; set; }

        /// <summary>
        /// Коллекция прав, запрашиваемых при авторизации EVE SSO.
        /// </summary>
        public string[] Scopes { get; set; }

        /// <summary>
        /// Дата и время последнего обновления БД налоговой отчетности.
        /// </summary>
        public DateTime LastUpdateDateTime { get; set; }

        /// <summary>
        /// Число часов, которое должно пройти между обновлением БД налоговой отчетности.
        /// </summary>
        public int HoursBeforeUpdate { get; set; }

        /// <summary>
        /// Ссылка на цену предметов сайта fuzzwork.
        /// </summary>
        public string FuzzworkPricesUrl { get; set; }

        /// <summary>
        /// Контейнер, описывающий параметры подсчёта налогов.
        /// </summary>
        public TaxParams TaxParams { get; set; }

        /// <summary>
        /// Контейнер, описывающий параметры взаимодействия с SEAT.
        /// </summary>
        public SeatParams SeatParams { get; set; }

        /// <summary>
        /// Метод записи конфига в файл.
        /// </summary>
        public void Write();
    }
}
