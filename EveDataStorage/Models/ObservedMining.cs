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
    /// <summary>
    /// Сущность добычи лунной руды.
    /// </summary>
    [Table("ObservedMinings")]
    public class ObservedMining : IOreModel
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <inheritdoc/>
        public int CharacterId { get; set; }

        /// <inheritdoc/>
        public DateTime LastUpdated { get; set; }

        /// <inheritdoc/>
        public long Quantity { get; set; }

        /// <inheritdoc/>
        public int CorporationId { get; set; }

        /// <inheritdoc/>
        public int TypeId { get; set; }

        /// <summary>
        /// Id структуры, где была добыча руды.
        /// </summary>
        public long ObserverId { get; set; }

        /// <inheritdoc/>
        public Character? Character { get; set; }

        /// <inheritdoc/>
        public Corporation? Corporation { get; set; }

        [NotMapped]
        private string _hash;

        /// <inheritdoc/>
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
