using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveDataStorage.Models
{
    [Table("Regions")]
    public class Region
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    [Table("Constellations")]
    public class Constellation
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int RegionId { get; set; }
        [Required]
        public Region Region { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    [Table("SolarSystems")]
    public class SolarSystem
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int ConstellationId { get; set; }
        [Required]
        public Constellation Constellation { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
