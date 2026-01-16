using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticDataStorage.Models
{
    /// <summary>
    /// Группа сходных объектов, вроде "персонаж", "сектор", "созвездие", "минералы", "титан".
    /// </summary>
    [Table("Groups")]
    public class Group
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Key { get; set; }

        public int Id { get; set; }

        public bool Anchorable { get; set; }

        public bool Anchored { get; set; }

        public int CategoryID { get; set; }

        public bool FittableNonSingleton { get; set; }

        public int IconID { get; set; }

        public string Name { get; set; }

        public bool Published { get; set; }

        public bool UseBasePrice { get; set; }

        [NotMapped]
        public Category Category { get; private set; }

        public static Group Empty()
        {
            return new Group()
            {
                Id = -1,
                Anchorable = false,
                Anchored = false,
                CategoryID = -1,
                FittableNonSingleton = false,
                IconID = -1,
                Name = string.Empty,
                Published = false,
                UseBasePrice = false,
            };
        }

        public void FillFrom(EveSdeModel.Models.Group group)
        {
            if (int.TryParse(group.Id, out int _id)) Id = _id;
            if (bool.TryParse(group.Anchorable, out bool _anchorable)) Anchorable = _anchorable;
            if (bool.TryParse(group.Anchored, out bool _anchored)) Anchored = _anchored;
            if (int.TryParse(group.CategoryID, out int _categoryID)) CategoryID = _categoryID;
            if (bool.TryParse(group.FittableNonSingleton, out bool _fittableNonSingleton)) FittableNonSingleton = _fittableNonSingleton;
            if (int.TryParse(group.IconID, out int _iconID)) IconID = _iconID;
            if (bool.TryParse(group.Published, out bool _published)) Published = _published;
            if (bool.TryParse(group.UseBasePrice, out bool _useBasePrice)) UseBasePrice = _useBasePrice;
            if (group.Name != null) this.Name = group.Name.English;
        }

        /// <summary>
        /// Заполняет поле категории группы из списка категорий.
        /// </summary>
        /// <param name="items">Список всех категорий.</param>
        public void FillCategories(IEnumerable<Category> categories)
        {
            var found = categories.FirstOrDefault(x => x.Id == CategoryID);
            if (found != null)
            {
                Category = found;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
