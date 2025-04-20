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
    [Table("MiningObservers")]
    public class MiningObserver
    {
        [Key]
        public int Id { get; set; }

        public DateTime LastUpdated { get; set; }

        public long ObserverId { get; set; }
    }
}
