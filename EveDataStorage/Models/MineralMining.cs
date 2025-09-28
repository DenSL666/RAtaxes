using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveDataStorage.Models
{
    /// <summary>
    /// Сущность добычи минеральной руды.
    /// </summary>
    [Table("MineralMinings")]
    public class MineralMining : IOreModel
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
        /// Id системы, где была добыта руда.
        /// </summary>
        public int SolarSystemId { get; set; }

        /// <inheritdoc/>
        public Character? Character { get; set; }

        /// <inheritdoc/>
        public Corporation? Corporation { get; set; }

        /// <summary>
        /// Сущность системы, где была добыта руда.
        /// </summary>
        public SolarSystem? SolarSystem { get; set; }

        [NotMapped]
        private string _hash;

        /// <inheritdoc/>
        [NotMapped]
        public string Hash
        {
            get
            {
                if (string.IsNullOrEmpty(_hash))
                    _hash = GetHash(CharacterId, LastUpdated, SolarSystemId, TypeId, CorporationId, Quantity);
                return _hash;
            }
        }

        public static string GetHash(int charId, DateTime date, long solarSystemId, int typeId, int corporationId, long quantity) => 
            string.Join('_', charId.ToString(), date.Ticks.ToString(), solarSystemId.ToString(), typeId.ToString(), corporationId.ToString(), quantity.ToString());
    }
}
