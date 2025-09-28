using EveWebClient.Esi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveWebClient.External.Models
{
    /// <summary>
    /// Класс данных о текущих ценах для некоторого предмета.
    /// </summary>
    public sealed class Price
    {
        internal Price()
        {
            Id = string.Empty;
        }

        private int? _id;
        public int TypeId
        {
            get
            {
                if (string.IsNullOrEmpty(Id))
                {
                    return -1;
                }
                else
                {
                    if (!_id.HasValue && int.TryParse(Id, out int _val))
                    {
                        _id = _val;
                    }
                    if (_id.HasValue)
                        return _id.Value;
                    else
                        return -1;
                }
            }
        }

        /// <summary>
        /// Id предмета.
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// Цена наибольшего жита бай согласно Fuzzwork.
        /// </summary>
        public double JitaBuy { get; internal set; }
        /// <summary>
        /// Цена наименьшего жита селл согласно Fuzzwork.
        /// </summary>
        public double JitaSell { get; internal set; }
        /// <summary>
        /// Цена жита сплит (средняя между бай и селл согласно Fuzzwork).
        /// </summary>
        public double JitaSplit { get; internal set; }
        /// <summary>
        /// Цена средняя по EVE согласно Esi.
        /// </summary>
        public double EveAverage { get; internal set; }
    }
}
