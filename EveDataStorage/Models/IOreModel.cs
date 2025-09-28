using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveDataStorage.Models
{
    /// <summary>
    /// Описывает модель добычи персонажем какого-либо типа руды в какой-то день в составе какой-то корпорации.
    /// </summary>
    public interface IOreModel
    {
        /// <summary>
        /// Id персонажа.
        /// </summary>
        public int CharacterId { get; }

        /// <summary>
        /// Дата добычи руды.
        /// </summary>
        public DateTime LastUpdated { get; }

        /// <summary>
        /// Количество добытой руды.
        /// </summary>
        public long Quantity { get; }

        /// <summary>
        /// Id корпорации персонажа, в которой он добыл руду.
        /// </summary>
        public int CorporationId { get; }

        /// <summary>
        /// Id типа добытой руды.
        /// </summary>
        public int TypeId { get; }

        /// <summary>
        /// Сущность персонажа.
        /// </summary>
        public Character? Character { get; }

        /// <summary>
        /// Сущность корпорации.
        /// </summary>
        public Corporation? Corporation { get; }

        /// <summary>
        /// Хэш строка, описывающая запись о добытой руде.
        /// </summary>
        public string Hash { get; }
    }
}
