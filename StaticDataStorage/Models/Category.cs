using EveSdeModel.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace StaticDataStorage.Models
{
    /// <summary>
    /// Крупные категории объектов, вроде "корабль", "дрон", "станция", "орбитальное тело".
    /// </summary>
    [Table("Categories")]
    public class Category
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Key { get; set; }

        public int Id { get; set; }

        public int IconID { get; set; }

        public string Name { get; set; }

        public bool Published { get; set; }

        public static Category Empty()
        {
            return new Category()
            {
                Id = -1,
                IconID = -1,
                Name = string.Empty,
                Published = false,
            };
        }

        public void FillFrom(EveSdeModel.Models.Category category)
        {
            if (int.TryParse(category.Id, out int _id)) Id = _id;
            if (int.TryParse(category.IconID, out int _iconID)) IconID = _iconID;
            if (bool.TryParse(category.Published, out bool _published)) Published = _published;
            if (category.Name != null) this.Name = category.Name.English;
        }
    }
}
