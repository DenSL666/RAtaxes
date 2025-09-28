using EveSdeModel.Factories;
using EveSdeModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace EveSdeModel.Models
{
    /// <summary>
    /// Описывает типы сущностей, вроде конкретных кораблей, модулей или минерал.
    /// </summary>
    public class EntityType : IYamlEntity
    {
        [YamlIgnore]
        private int? _id;

        /// <summary>
        /// Числовый вид Id сущности.
        /// </summary>
        [YamlIgnore]
        public int TypeId
        {
            get
            {
                if (string.IsNullOrEmpty(Id))
                {
                    return -1;
                }
                else
                {
                    if (!_id.HasValue && int.TryParse(Id, out int _val))
                    {
                        _id = _val;
                    }
                    if (_id.HasValue)
                        return _id.Value;
                    else
                        return -1;
                }
            }
        }

        /// <summary>
        /// Id сущности.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Масса сущности.
        /// </summary>
        [JsonPropertyName("mass")]
        public string Mass { get; set; }

        /// <summary>
        /// Минимальное количество сущностей, которые могут быть разобраны на материалы.<br/>
        /// Например, руды разбираются не менее, чем 100 единиц.
        /// </summary>
        [JsonPropertyName("portionSize")]
        public string PortionSize { get; set; }

        /// <summary>
        /// Является ли сущность публичной, т.е. доступной игрокам.
        /// </summary>
        [JsonPropertyName("published")]
        public string Published { get; set; }

        /// <summary>
        /// Объем сущности.
        /// </summary>
        [JsonPropertyName("volume")]
        public string Volume { get; set; }

        /// <summary>
        /// Радиус сущности.
        /// </summary>
        [JsonPropertyName("radius")]
        public string Radius { get; set; }

        /// <summary>
        /// Id иконки в игре.
        /// </summary>
        [JsonPropertyName("iconID")]
        public string IconID { get; set; }

        /// <summary>
        /// Id группы.
        /// </summary>
        [JsonPropertyName("groupID")]
        public string GroupID { get; set; }

        [JsonPropertyName("graphicID")]
        public string GraphicID { get; set; }

        [JsonPropertyName("soundID")]
        public string SoundID { get; set; }

        [JsonPropertyName("raceID")]
        public string RaceID { get; set; }

        [JsonPropertyName("sofFactionName")]
        public string SofFactionName { get; set; }

        [JsonPropertyName("basePrice")]
        public string BasePrice { get; set; }

        [JsonPropertyName("marketGroupID")]
        public string MarketGroupID { get; set; }

        [JsonPropertyName("capacity")]
        public string Capacity { get; set; }

        [JsonPropertyName("metaGroupID")]
        public string MetaGroupID { get; set; }

        [JsonPropertyName("variationParentTypeID")]
        public string VariationParentTypeID { get; set; }

        [JsonPropertyName("factionID")]
        public string FactionID { get; set; }

        [JsonPropertyName("sofMaterialSetID")]
        public string SofMaterialSetID { get; set; }

        /// <summary>
        /// Контейнер, описывающий имя сущности на разных языках.
        /// </summary>
        [JsonPropertyName("name")]
        public Name Name { get; set; }

        /// <summary>
        /// Контейнер, описывающий сущность в игре на разных языках.<br/>
        /// Не читаю его, чтобы не сохранять при перезаписи файла SDE.
        /// </summary>
        [JsonPropertyName("description")]
        public Name Description { get; set; }

        /// <summary>
        /// Группа сущности.
        /// </summary>
        public Group Group { get; private set; }

        public EntityType()
        {
            Id = string.Empty;
            Mass = string.Empty;
            PortionSize = string.Empty;
            Published = string.Empty;
            Volume = string.Empty;
            Radius = string.Empty;
            IconID = string.Empty;
            GroupID = string.Empty;
            GraphicID = string.Empty;
            SoundID = string.Empty;
            RaceID = string.Empty;
            SofFactionName = string.Empty;
            BasePrice = string.Empty;
            MarketGroupID = string.Empty;
            Capacity = string.Empty;
            MetaGroupID = string.Empty;
            VariationParentTypeID = string.Empty;
            FactionID = string.Empty;
            SofMaterialSetID = string.Empty;

            Group = new Group();
            Name = new Name();
            Description = new Name();
        }

        protected EntityType(EntityType entity)
        {
            Id = new string(entity.Id.ToCharArray());
            Mass = new string(entity.Mass.ToCharArray());
            PortionSize = new string(entity.PortionSize.ToCharArray());
            Published = new string(entity.Published.ToCharArray());
            Volume = new string(entity.Volume.ToCharArray());
            Radius = new string(entity.Radius.ToCharArray());
            IconID = new string(entity.IconID.ToCharArray());
            GroupID = new string(entity.GroupID.ToCharArray());
            GraphicID = new string(entity.GraphicID.ToCharArray());
            SoundID = new string(entity.SoundID.ToCharArray());
            RaceID = new string(entity.RaceID.ToCharArray());
            SofFactionName = new string(entity.SofFactionName.ToCharArray());
            BasePrice = new string(entity.BasePrice.ToCharArray());
            MarketGroupID = new string(entity.MarketGroupID.ToCharArray());
            Capacity = new string(entity.Capacity.ToCharArray());
            MetaGroupID = new string(entity.MetaGroupID.ToCharArray());
            VariationParentTypeID = new string(entity.VariationParentTypeID.ToCharArray());
            FactionID = new string(entity.FactionID.ToCharArray());
            SofMaterialSetID = new string(entity.SofMaterialSetID.ToCharArray());
            Name = entity.Name.DeepCopy();
            Description = entity.Description.DeepCopy();
            Group = entity.Group.DeepCopy();
        }

        /// <summary>
        /// Является объект опубликованным (доступным игрокам).
        /// </summary>
        [YamlIgnore]
        public bool IsPublished => !string.IsNullOrEmpty(Published) && Published == "true";

        public void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode)
        {
            Id = yamlNode.Key.ToString();
            var properties = EveYamlFactory.GetProperties(GetType());
            foreach (var node in ((YamlMappingNode)yamlNode.Value).Children)
            {
                var found = properties.FirstOrDefault(x => x.Name.GetAttr<EntityType>() == node.Key.ToString());
                if (found != null)
                {
                    found.SetValue(this, node.Value.ToString());
                }
                if (node.Key.ToString() == nameof(Name).GetAttr<EntityType>())
                {
                    Name = EveYamlFactory.GetObject<Name>((YamlMappingNode)node.Value);
                }
                //  убираю чтение описания
                //if (node.Key.ToString() == nameof(Description).GetAttr<EntityType>())
                //{
                //    Description = EveYamlFactory.GetObject<Name>((YamlMappingNode)node.Value);
                //}
            }
        }

        public void ParseNoId(YamlMappingNode yamlNode)
        {
            throw new NotImplementedException();
        }

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
            if (Name == null || string.IsNullOrEmpty(Name.English))
                return 0;
            if (FactionNavy.Any(x => Name.English.Contains(x)))
                return 2;
            if (Ships_2.Contains(Name.English))
                return 2;
            if (Modules_2.Any(x => Name.English.Contains(x)))
                return 2;
            if (Name.English.EndsWith(" II"))
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

        public string Write() => $"{Name.English}\t{Group?.Category?.Name?.English} {GetTech()}\t{Group?.Name?.English}";

        public override string ToString()
        {
            return Name?.English ?? "";
        }

        public static EntityType DeepCopy(EntityType entity)
        {
            return new EntityType(entity);
        }

        public EntityType DeepCopy()
        {
            return new EntityType(this);
        }
    }
}
