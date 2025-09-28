using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveDataStorage.Models
{
    /// <summary>
    /// Сущность региона.
    /// </summary>
    [Table("Regions")]
    public class Region
    {
        /// <summary>
        /// Id региона.
        /// </summary>
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Имя региона.
        /// </summary>
        [Required]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Сущность созвездия.
    /// </summary>
    [Table("Constellations")]
    public class Constellation
    {
        /// <summary>
        /// Id созвездия.
        /// </summary>
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Имя созвездия.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Id региона.
        /// </summary>
        [Required]
        public int RegionId { get; set; }

        /// <summary>
        /// Сущность региона.
        /// </summary>
        [Required]
        public Region Region { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Сущность солнечной системы.
    /// </summary>
    [Table("SolarSystems")]
    public class SolarSystem
    {
        /// <summary>
        /// Id системы.
        /// </summary>
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Имя системы.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Id созвездия.
        /// </summary>
        [Required]
        public int ConstellationId { get; set; }

        /// <summary>
        /// Сущность созвездия.
        /// </summary>
        [Required]
        public Constellation Constellation { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
