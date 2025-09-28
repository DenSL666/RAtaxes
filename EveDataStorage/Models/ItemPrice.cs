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
    /// Сущность цены объекта.
    /// </summary>
    [Table("ItemPrices")]
    public class ItemPrice
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Id сущности объекта в EVE.
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Дата получения цены.
        /// </summary>
        public DateTime DateUpdate { get; set; }

        /// <summary>
        /// Средняя цена предмета по всей EVE.
        /// </summary>
        public double AveragePrice { get; set; }

        /// <summary>
        /// Наибольшая цена скупки предмета в Jita.
        /// </summary>
        public double JitaBuyPrice { get; set; }

        /// <summary>
        /// Наименьшая цена продажи предмета в Jita.
        /// </summary>
        public double JitaSellPrice { get; set; }

        /// <summary>
        /// Среднее значение цены между скупкой и продажей.
        /// </summary>
        [NotMapped]
        public double JitaSplit => (JitaSellPrice + JitaBuyPrice) / 2;
    }
}
