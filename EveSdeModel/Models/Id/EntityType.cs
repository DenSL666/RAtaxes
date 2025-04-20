using EveSdeModel.Interfaces;
using EveSdeModel.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YamlDotNet.RepresentationModel;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace EveSdeModel.Models
{
    public class EntityType : IYamlEntity
    {
        [YamlIgnore]
        private int? _id;

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

        public string Id { get; set; }

        [JsonPropertyName("mass")]
        public string Mass { get; set; }

        [JsonPropertyName("portionSize")]
        public string PortionSize { get; set; }

        [JsonPropertyName("published")]
        public string Published { get; set; }

        [JsonPropertyName("volume")]
        public string Volume { get; set; }

        [JsonPropertyName("radius")]
        public string Radius { get; set; }

        [JsonPropertyName("iconID")]
        public string IconID { get; set; }

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

        [JsonPropertyName("name")]
        public Name Name { get; set; }

        [JsonPropertyName("description")]
        public Name Description { get; set; }

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

        private static List<string> Ships_2 = new List<string>
        {
            "Imperial Navy Slicer", "Caldari Navy Hookbill", "Federation Navy Comet", "Republic Fleet Firetail", "Magnate Navy Issue", "Heron Navy Issue", "Imicus Navy Issue", "Probe Fleet Issue",
            "Crucifier Navy Issue", "Griffin Navy Issue", "Maulus Navy Issue", "Vigil Fleet Issue", "Astero", "Cruor", "Daredevil", "Dramiel", "Garmur", "Succubus", "Worm", "Sentinel", "Kitsune",
            "Keres", "Hyena", "Malediction", "Crow", "Ares", "Stiletto", "Imp", "Whiptail", "Crusader", "Raptor", "Taranis", "Claw", "Anathema", "Buzzard", "Helios", "Cheetah", "Purifier", "Manticore",
            "Nemesis", "Hound", "Vengeance", "Harpy", "Hawk", "Jaguar", "Nergal", "Retribution", "Enyo", "Ishkur", "Wolf", "Deacon", "Kirin", "Thalia", "Scalpel", "Endurance", "Prospect", "Kikimora",
            "Coercer Navy Issue", "Cormorant Navy Issue", "Catalyst Navy Issue", "Thrasher Fleet Issue", "Mamba", "Mekubal", "Heretic", "Flycatcher", "Eris", "Sabre", "Pontifex", "Bifrost", "Stork", "Magus",
            "Draugur", "Confessor", "Svipul", "Jackdaw", "Hecate", "Rodiva", "Vedmak", "Augoror Navy Issue", "Omen Navy Issue", "Caracal Navy Issue", "Osprey Navy Issue", "Exequror Navy Issue", "Vexor Navy Issue",
            "Scythe Fleet Issue", "Stabber Fleet Issue", "Ashimmu", "Cynabal", "Gila", "Orthrus", "Phantasm", "Stratios", "Vigilant", "Pilgrim", "Curse", "Falcon", "Rook", "Arazu", "Lachesis", "Rapier", "Huginn",
            "Zealot", "Sacrilege", "Cerberus", "Eagle", "Ishtar", "Deimos", "Muninn", "Vagabond", "Ikitursa", "Devoter", "Onyx", "Phobos", "Broadsword", "Guardian", "Basilisk", "Oneiros", "Scimitar", "Zarmazd",
            "Monitor", "Legion", "Tengu", "Proteus", "Loki", "Harbinger Navy Issue", "Prophecy Navy Issue", "Drake Navy Issue", "Ferox Navy Issue", "Brutix Navy Issue", "Myrmidon Navy Issue", "Hurricane Fleet Issue",
            "Cyclone Fleet Issue", "Absolution", "Nighthawk", "Astarte", "Sleipnir", "Damnation", "Vulture", "Eos", "Claymore", "Redeemer", "Widow", "Sin", "Panther", "Marshal", "Ark", "Rhea", "Anshar", "Nomad",
            "Vehement", "Chemosh", "Caiman", "Zirnitra", "Revelation Navy Issue", "Phoenix Navy Issue", "Moros Navy Issue", "Naglfar Fleet Issue", "Dagon", "Loggerhead", "Revenant", "Vendetta", "Vanquisher", "Molok", "Komodo",
            "Azariel", "Alligator", "Bane", "Bhaalgorn", "Bustard", "Crane", "Damavik", "Deluge", "Torrent", "Drekavac", "Enforcer", "Golem", "Hubris", "Hulk", "Impel", "Karura", "Khizriel", "Kronos", "Leshak",
            "Machariel", "Mackinaw", "Mamba", "Mastodon", "Nestor", "Nightmare", "Occator", "Pacifier", "Paladin", "Prorator", "Prowler", "Rattlesnake", "Skiff", "Skybreaker", "Stormbringer", "Thunderchild",
            "Valravn", "Vargur", "Viator", "Vindicator"
        };

        private static List<string> Modules_2 = new List<string>
        {
            "Null", "Void", "Spike", "Javelin", "Barrage", "Hail", "Quake", "Tremor", "Scorch", "Conflagration", "Aurora", "Gleam", "Tetryon", "Baryon", "Meson", "Mystic", "Occult", "Imperial Navy", "Ammatar Navy", "Caldari Navy",
            "Dark Blood", "Domination", "Dread Guristas", "Federation Navy", "Republic Fleet", "Sisters", "Shadow Serpentis", "True Sansha", "Veles", "Integrated", "Augmented", "Harvester", "Excavator", "Precision", "Fury", "High-grade", "Mid-grade", "Low-grade", 
            "Navy Issue", "Fleet Issue",
        };

        public int GetTech()
        {
            if (Name == null || string.IsNullOrEmpty(Name.English))
                return 0;
            if (Ships_2.Contains(Name.English))
                return 2;
            if (Modules_2.Any(x => Name.English.Contains(x)))
                return 2;
            if (Name.English.EndsWith(" II"))
                return 2;
            return 1;
        }

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

        [YamlIgnore]
        public static List<string> Cats = new List<string>
        {
            "Charge 1",
            "Charge 2",
            "Charge 3",
            "Module 1",
            "Module 2",
            "Subsystem 1",
            "Subsystem 2",
            "Ship 1",
            "Ship 2",
            "Drone 1",
            "Drone 2",
            "Infrastructure Upgrades 1",
            "Implant 1",
            "Implant 2",
            "Fighter 1",
            "Fighter 2",
            "Celestial 1",
            "Commodity 1",
            "Commodity 2",
            "Orbitals 1",
            "Deployable 1",
            "Deployable 2",
            "Starbase 1",
            "Starbase 2",
            "Entity 1",
            "Sovereignty Structures 1",
            "Special Edition Assets 1",
            "Structure 1",
            "Structure Module 1",
            "Structure Module 2",
            "Formula",
        };
    }
}
