using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveDataStorage.Models
{
    public interface IOreModel
    {
        public int CharacterId { get; }

        public DateTime LastUpdated { get; }

        public long Quantity { get; }

        public int CorporationId { get; }

        public int TypeId { get; }

        public Character? Character { get; }

        public Corporation? Corporation { get; }

        public string Hash { get; }
    }
}
