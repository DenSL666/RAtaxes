using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveWebClient.Esi.Models
{
    /// <summary>
    /// Обёртка для данных EVE Esi.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ESIModelDTO<T>
    {
        /// <summary>
        /// Данные EVE Esi.
        /// </summary>
        public T Model { get; set; }
        public bool NotModified { get; set; }
        public string ETag { get; set; }
        public string Language { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public int MaxPages { get; set; }
        public int RemainingErrors { get; set; }

        /// <summary>
        /// Id объекта, который описан в <see cref="Model"/> .
        /// </summary>
        public int ObjectId { get; set; }
    }
}
