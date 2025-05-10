using EveWebClient.Esi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveWebClient.External.Models
{
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

        public string Id { get; internal set; }
        public double JitaBuy { get; internal set; }
        public double JitaSell { get; internal set; }
        public double JitaSplit { get; internal set; }
        public double EveAverage { get; internal set; }
    }
}
