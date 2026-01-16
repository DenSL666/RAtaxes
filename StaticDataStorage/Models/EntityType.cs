using EveSdeModel.Models;
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
    /// Описывает типы сущностей, вроде конкретных кораблей, модулей или минерал.
    /// </summary>
    [Table("EntityTypes")]
    public class EntityType
    {
        /// <summary>
        /// Id записи в БД.
        /// </summary>
        [Key]
        public int Key { get; set; }

        /// <summary>
        /// Id сущности.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Масса сущности.
        /// </summary>
        public long Mass { get; set; }

        /// <summary>
        /// Минимальное количество сущностей, которые могут быть разобраны на материалы.<br/>
        /// Например, руды разбираются не менее, чем 100 единиц.
        /// </summary>
        public int PortionSize { get; set; }

        /// <summary>
        /// Является ли сущность публичной, т.е. доступной игрокам.
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Объем сущности.
        /// </summary>
        public float Volume { get; set; }

        /// <summary>
        /// Радиус сущности.
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// Id иконки в игре.
        /// </summary>
        public int IconID { get; set; }

        /// <summary>
        /// Id группы.
        /// </summary>
        public int GroupID { get; set; }

        public int GraphicID { get; set; }

        public int SoundID { get; set; }

        public int RaceID { get; set; }

        public string SofFactionName { get; set; }

        public float BasePrice { get; set; }

        public int MarketGroupID { get; set; }

        public float Capacity { get; set; }

        public int MetaGroupID { get; set; }

        public int VariationParentTypeID { get; set; }

        public int FactionID { get; set; }

        public int SofMaterialSetID { get; set; }

        /// <summary>
        /// Имя сущности на английском языке.
        /// </summary>
        public string NameEnglish { get; set; }

        /// <summary>
        /// Имя сущности на русском языке.
        /// </summary>
        public string NameRussian { get; set; }

        /// <summary>
        /// Группа сущности.
        /// </summary>
        [NotMapped]
        public Group Group { get; private set; }

        public static EntityType Empty()
        {
            return new EntityType()
            {
                Id = -1,
                Mass = -1,
                PortionSize = -1,
                Published = false,
                Volume = -1,
                Radius = -1,
                IconID = -1,
                GroupID = -1,
                GraphicID = -1,
                SoundID = -1,
                RaceID = -1,
                SofFactionName = string.Empty,
                BasePrice = -1,
                MarketGroupID = -1,
                Capacity = -1,
                MetaGroupID = -1,
                VariationParentTypeID = -1,
                FactionID = -1,
                SofMaterialSetID = -1,
                NameEnglish = string.Empty,
                NameRussian = string.Empty,
            };
        }

        public void FillFrom(EveSdeModel.Models.EntityType entityType)
        {
            if (int.TryParse(entityType.Id, out int _id)) Id = _id;
            if (int.TryParse(entityType.Mass, out int _mass)) Mass = _mass;
            if (int.TryParse(entityType.PortionSize, out int _portionSize)) PortionSize = _portionSize;
            if (bool.TryParse(entityType.Published, out bool _published)) Published = _published;
            if (int.TryParse(entityType.Volume, out int _volume)) Volume = _volume;
            if (int.TryParse(entityType.Radius, out int _radius)) Radius = _radius;
            if (int.TryParse(entityType.IconID, out int _iconID)) IconID = _iconID;
            if (int.TryParse(entityType.GroupID, out int _groupID)) GroupID = _groupID;
            if (int.TryParse(entityType.GraphicID, out int _graphicID)) GraphicID = _graphicID;
            if (int.TryParse(entityType.SoundID, out int _soundID)) SoundID = _soundID;
            if (int.TryParse(entityType.RaceID, out int _raceID)) RaceID = _raceID;
            
            if (int.TryParse(entityType.BasePrice, out int _basePrice)) BasePrice = _basePrice;
            if (int.TryParse(entityType.MarketGroupID, out int _marketGroupID)) MarketGroupID = _marketGroupID;
            if (int.TryParse(entityType.Capacity, out int _capacity)) Capacity = _capacity;
            if (int.TryParse(entityType.MetaGroupID, out int _metaGroupID)) MetaGroupID = _metaGroupID;
            if (int.TryParse(entityType.VariationParentTypeID, out int _variationParentTypeID)) VariationParentTypeID = _variationParentTypeID;
            if (int.TryParse(entityType.FactionID, out int _factionID)) FactionID = _factionID;
            if (int.TryParse(entityType.SofMaterialSetID, out int _sofMaterialSetID)) SofMaterialSetID = _sofMaterialSetID;

            SofFactionName = entityType.SofFactionName;
            if (entityType.Name != null)
            {
                NameEnglish = entityType.Name.English;
                NameRussian = entityType.Name.Russian;
            }
        }

        /// <summary>
        /// Заполняет поле группы сущности из списка групп.
        /// </summary>
        /// <param name="groups">Список групп.</param>
        public void FillGroups(IReadOnlyCollection<Group> groups)
        {
            var found = groups.FirstOrDefault(x => x.Id == GroupID);
            if (found != null)
            {
                Group = found;
            }
        }

        #region Get Tech Level

        /// <summary>
        /// Список префиксов имени, содержащийся в т1 фракционных вариантах сущноостей.
        /// </summary>
        private static List<string> FactionNavy = new List<string>
        {
            "Navy", "Fleet",
        };

        /// <summary>
        /// Имена т2 кораблей амарской фракции.
        /// </summary>
        private static List<string> AmarrT2 = new List<string>
        {
            "Retribution",
            "Vengeance",
            "Anathema",
            "Purifier",
            "Sentinel",
            "Crusader",
            "Malediction",
            "Deacon",
            "Pontifex",
            "Heretic",
            "Confessor",
            "Sacrilege",
            "Zealot",
            "Devoter",
            "Guardian",
            "Curse",
            "Pilgrim",
            "Legion",
            "Absolution",
            "Damnation",
            "Redeemer",
            "Paladin",
            "Prorator",
            "Impel",
            "Ark",
            "Bane",
        };

        /// <summary>
        /// Имена т2 кораблей минматарской фракции.
        /// </summary>
        private static List<string> MinmatarT2 = new List<string>
        {
            "Jaguar",
            "Wolf",
            "Cheetah",
            "Hound",
            "Hyena",
            "Claw",
            "Stiletto",
            "Scalpel",
            "Bifrost",
            "Sabre",
            "Svipul",
            "Muninn",
            "Vagabond",
            "Broadsword",
            "Scimitar",
            "Huginn",
            "Rapier",
            "Loki",
            "Claymore",
            "Sleipnir",
            "Panther",
            "Vargur",
            "Prowler",
            "Mastodon",
            "Nomad",
            "Valravn",
        };

        /// <summary>
        /// Имена т2 кораблей калдарской фракции.
        /// </summary>
        private static List<string> CaldariT2 = new List<string>
        {
            "Harpy",
            "Hawk",
            "Buzzard",
            "Manticore",
            "Kitsune",
            "Crow",
            "Raptor",
            "Kirin",
            "Stork",
            "Flycatcher",
            "Jackdaw",
            "Cerberus",
            "Eagle",
            "Onyx",
            "Basilisk",
            "Falcon",
            "Rook",
            "Tengu",
            "Nighthawk",
            "Vulture",
            "Widow",
            "Golem",
            "Crane",
            "Bustard",
            "Rhea",
            "Karura",
        };

        /// <summary>
        /// Имена т2 кораблей галлентской фракции.
        /// </summary>
        private static List<string> GallenteT2 = new List<string>
        {
            "Enyo",
            "Ishkur",
            "Helios",
            "Nemesis",
            "Keres",
            "Ares",
            "Taranis",
            "Thalia",
            "Magus",
            "Eris",
            "Hecate",
            "Deimos",
            "Ishtar",
            "Phobos",
            "Oneiros",
            "Arazu",
            "Lachesis",
            "Proteus",
            "Astarte",
            "Eos",
            "Sin",
            "Kronos",
            "Viator",
            "Occator",
            "Anshar",
            "Hubris",
        };

        /// <summary>
        /// Имена т2 кораблей рудной фракции.
        /// </summary>
        private static List<string> OreT2 = new List<string>
        {
            "Prospect",
            "Endurance",
            "Hulk",
            "Skiff",
            "Mackinaw",
        };

        /// <summary>
        /// Имена т2 кораблей фракции пиратов гуристас.
        /// </summary>
        private static List<string> Guristas = new List<string>
        {
            "Worm",
            "Mamba",
            "Gila",
            "Alligator",
            "Rattlesnake",
            "Caiman",
            "Loggerhead",
            "Komodo",
        };

        /// <summary>
        /// Имена т2 кораблей фракции саньши.
        /// </summary>
        private static List<string> Sansha = new List<string>
        {
            "Succubus",
            "Phantasm",
            "Nightmare",
            "Revenant",
        };

        /// <summary>
        /// Имена т2 кораблей фракции кровавых рейдеров.
        /// </summary>
        private static List<string> Blood = new List<string>
        {
            "Cruor",
            "Ashimmu",
            "Bhaalgorn",
            "Chemosh",
            "Dagon",
            "Molok",
        };

        /// <summary>
        /// Имена т2 кораблей фракции пиратов ангелов.
        /// </summary>
        private static List<string> Angel = new List<string>
        {
            "Dramiel",
            "Mekubal",
            "Cynabal",
            "Khizriel",
            "Machariel",
            "Sarathiel",
            "Azariel",
        };

        /// <summary>
        /// Имена т2 кораблей фракции серпентис.
        /// </summary>
        private static List<string> Serpentis = new List<string>
        {
            "Daredevil",
            "Vigilant",
            "Vindicator",
            "Vehement",
            "Vendetta",
            "Vanquisher",
        };

        /// <summary>
        /// Имена т2 кораблей фракции сестёр евы.
        /// </summary>
        private static List<string> Sisters = new List<string>
        {
            "Astero",
            "Stratios",
            "Nestor",
        };

        /// <summary>
        /// Имена т2 кораблей фракции легион морду.
        /// </summary>
        private static List<string> Mordu = new List<string>
        {
            "Garmur",
            "Orthrus",
            "Barghest",
        };

        /// <summary>
        /// Имена т2 кораблей триглавской фракции.
        /// </summary>
        private static List<string> Triglav = new List<string>
        {
            "Damavik",
            "Nergal",
            "Kikimora",
            "Draugur",
            "Vedmak",
            "Rodiva",
            "Ikitursa",
            "Zarmazd",
            "Drekavac",
            "Leshak",
            "Babaroga",
            "Zirnitra",
        };

        /// <summary>
        /// Имена т2 кораблей фракции эденком.
        /// </summary>
        private static List<string> Edencom = new List<string>
        {
            "Skybreaker",
            "Stormbringer",
            "Thunderchild",
            "Deluge",
            "Torrent",
        };

        /// <summary>
        /// Имена т2 кораблей фракции конкорд.
        /// </summary>
        private static List<string> Concord = new List<string>
        {
            "Pacifier",
            "Enforcer",
            "Monitor",
            "Marshal",
        };

        /// <summary>
        /// Имена т2 кораблей фракции бессмертных.
        /// </summary>
        private static List<string> Deathless = new List<string>
        {
            "Tholos",
            "Cenotaph",
        };

        /// <summary>
        /// Список имён кораблей, которые в своём чертеже зачастую содержат 0 материало-эффективность.
        /// </summary>
        private static List<string> Ships_2 =
            AmarrT2
            .Concat(MinmatarT2)
            .Concat(CaldariT2)
            .Concat(GallenteT2)
            .Concat(OreT2)
            .Concat(Guristas)
            .Concat(Sansha)
            .Concat(Blood)
            .Concat(Angel)
            .Concat(Serpentis)
            .Concat(Sisters)
            .Concat(Mordu)
            .Concat(Triglav)
            .Concat(Edencom)
            .Concat(Concord)
            .Concat(Deathless)
            .ToList();

        /// <summary>
        /// Список частей или целых имён модулей, которые в своём чертеже зачастую содержат 0 материало-эффективность.
        /// </summary>
        private static List<string> Modules_2 = new List<string>
        {
            "Null", "Void", "Spike", "Javelin", "Barrage", "Hail", "Quake", "Tremor", "Scorch", "Conflagration", "Aurora", "Gleam", "Tetryon", "Baryon", "Meson", "Mystic", "Occult", "Imperial Navy", "Ammatar Navy", "Caldari Navy",
            "Dark Blood", "Domination", "Dread Guristas", "Federation Navy", "Republic Fleet", "Sisters", "Shadow Serpentis", "True Sansha", "Veles", "Integrated", "Augmented", "Harvester", "Excavator", "Precision", "Fury", "High-grade", "Mid-grade", "Low-grade",
            "Navy Issue", "Fleet Issue",
        };

        /// <summary>
        /// Выбирает число 1 или 2, которое определяет, какая материало-эффективность чертежа текущей сущности (равна 0 или может быть увеличена до 10%).
        /// </summary>
        /// <returns></returns>
        public int GetTech()
        {
            if (string.IsNullOrEmpty(NameEnglish))
                return 0;
            if (FactionNavy.Any(x => NameEnglish.Contains(x)))
                return 2;
            if (Ships_2.Contains(NameEnglish))
                return 2;
            if (Modules_2.Any(x => NameEnglish.Contains(x)))
                return 2;
            if (NameEnglish.EndsWith(" II"))
                return 2;
            return 1;
        }

        /// <summary>
        /// Заполняет поле группы сущности из списка групп.
        /// </summary>
        /// <param name="groups">Список групп.</param>
        public void FillGroups(IEnumerable<Group> groups)
        {
            var found = groups.FirstOrDefault(x => x.Id == GroupID);
            if (found != null)
            {
                Group = found;
            }
        }

        #endregion

        public override string ToString()
        {
            return NameEnglish;
        }
    }
}
