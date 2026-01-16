using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace StaticDataStorage.Models
{
    /// <summary>
    /// Сущность текущей версии SDE на сервере EVE и в БД программы.
    /// </summary>
    [Table("SdeVersions")]
    public class SdeVersionData
    {
        [Key]
        [Column("alliance_id")]
        [JsonPropertyName("_key")]
        public string Key { get; set; } = string.Empty;

        [Required]
        [Column("buildNumber")]
        [JsonPropertyName("buildNumber")]
        public int BuildNumber { get; set; }

        [Column("releaseDate")]
        [JsonPropertyName("releaseDate")]
        public DateTime ReleaseDate { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is SdeVersionData data)
            {
                if (this == null || data == null)
                    return false;
                return this.BuildNumber == data.BuildNumber;
            }
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
