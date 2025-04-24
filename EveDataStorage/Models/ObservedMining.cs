using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EveDataStorage.Models
{
    [Table("ObservedMinings")]
    public class ObservedMining
    {
        [Key]
        public int Id { get; set; }

        public int CharacterId { get; set; }

        public DateTime LastUpdated { get; set; }

        public long Quantity { get; set; }

        public int CorporationId { get; set; }

        public int TypeId { get; set; }

        public long ObserverId { get; set; }

        public Character? Character { get; set; }

        public Corporation? Corporation { get; set; }

        [NotMapped]
        private string _hash;
        [NotMapped]
        public string Hash
        {
            get
            {
                if (string.IsNullOrEmpty(_hash))
                    _hash = GetHash(CharacterId, LastUpdated, ObserverId, TypeId);
                return _hash;
            }
        }
        public static string GetHash(int charId, DateTime date, long structureId, int typeId) => string.Join('_', charId.ToString(), date.Ticks.ToString(), structureId.ToString(), typeId.ToString());
    }
}
