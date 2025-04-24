using EveSdeModel.Factories;
using EveSdeModel.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace EveSdeModel.Serialization
{
    public sealed class EntityTypeConverter : IYamlTypeConverter
    {
        public IValueSerializer ValueSerializer { get; set; }
        public IValueDeserializer ValueDeserializer { get; set; }

        public bool Accepts(Type type) => type == typeof(EntityType);

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            throw new NotImplementedException();
            //parser.Consume<MappingStart>();

            //var call = new EntityType
            //{
            //    Id = (string)ValueDeserializer.DeserializeValue(parser, typeof(string), new SerializerState(), ValueDeserializer),
            //    Materials = (List<Material>)ValueDeserializer.DeserializeValue(parser, typeof(List<Material>), new SerializerState(), ValueDeserializer),
            //};

            //parser.Consume<MappingEnd>();

            //return call;
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            var call = (EntityType)value;

            emitter.Emit(new MappingStart());
            ValueSerializer.SerializeValue(emitter, call.Id, typeof(string));
            {
                emitter.Emit(new MappingStart(null, null, true, MappingStyle.Block));

                if (!string.IsNullOrEmpty(call.BasePrice))
                {
                    emitter.Emit(new Scalar(nameof(call.BasePrice).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.BasePrice));
                }

                if (!string.IsNullOrEmpty(call.Capacity))
                {
                    emitter.Emit(new Scalar(nameof(call.Capacity).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.Capacity));
                }

                if (!string.IsNullOrEmpty(call.FactionID))
                {
                    emitter.Emit(new Scalar(nameof(call.FactionID).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.FactionID));
                }

                if (!string.IsNullOrEmpty(call.GraphicID))
                {
                    emitter.Emit(new Scalar(nameof(call.GraphicID).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.GraphicID));
                }

                if (!string.IsNullOrEmpty(call.GroupID))
                {
                    emitter.Emit(new Scalar(nameof(call.GroupID).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.GroupID));
                }

                if (!string.IsNullOrEmpty(call.IconID))
                {
                    emitter.Emit(new Scalar(nameof(call.IconID).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.IconID));
                }

                if (!string.IsNullOrEmpty(call.MarketGroupID))
                {
                    emitter.Emit(new Scalar(nameof(call.MarketGroupID).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.MarketGroupID));
                }

                if (!string.IsNullOrEmpty(call.Mass))
                {
                    emitter.Emit(new Scalar(nameof(call.Mass).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.Mass));
                }

                if (!string.IsNullOrEmpty(call.MetaGroupID))
                {
                    emitter.Emit(new Scalar(nameof(call.MetaGroupID).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.MetaGroupID));
                }

                emitter.Emit(new Scalar(nameof(call.Name).GetAttr<EntityType>()));
                ValueSerializer.SerializeValue(emitter, call.Name, typeof(Name));

                if (!string.IsNullOrEmpty(call.PortionSize))
                {
                    emitter.Emit(new Scalar(nameof(call.PortionSize).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.PortionSize));
                }

                if (!string.IsNullOrEmpty(call.Published))
                {
                    emitter.Emit(new Scalar(nameof(call.Published).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.Published));
                }

                if (!string.IsNullOrEmpty(call.RaceID))
                {
                    emitter.Emit(new Scalar(nameof(call.RaceID).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.RaceID));
                }

                if (!string.IsNullOrEmpty(call.Radius))
                {
                    emitter.Emit(new Scalar(nameof(call.Radius).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.Radius));
                }

                if (!string.IsNullOrEmpty(call.SofFactionName))
                {
                    emitter.Emit(new Scalar(nameof(call.SofFactionName).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.SofFactionName));
                }

                if (!string.IsNullOrEmpty(call.SofMaterialSetID))
                {
                    emitter.Emit(new Scalar(nameof(call.SofMaterialSetID).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.SofMaterialSetID));
                }

                if (!string.IsNullOrEmpty(call.SoundID))
                {
                    emitter.Emit(new Scalar(nameof(call.SoundID).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.SoundID));
                }

                if (!string.IsNullOrEmpty(call.VariationParentTypeID))
                {
                    emitter.Emit(new Scalar(nameof(call.VariationParentTypeID).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.VariationParentTypeID));
                }

                if (!string.IsNullOrEmpty(call.Volume))
                {
                    emitter.Emit(new Scalar(nameof(call.Volume).GetAttr<EntityType>()));
                    emitter.Emit(new Scalar(call.Volume));
                }

                emitter.Emit(new MappingEnd());
            }

            emitter.Emit(new MappingEnd());
        }

        public static string SerializeEntityType(EntityType call)
        {
            var methodCallConverter = new EntityTypeConverter();
            var serializerBuilder = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(methodCallConverter);

            methodCallConverter.ValueSerializer = serializerBuilder.BuildValueSerializer();

            var serializer = serializerBuilder.Build();

            var yaml = serializer.Serialize(call);
            return yaml;
        }

        public static EntityType DeserializeEntityType(string yaml)
        {
            throw new NotImplementedException();
            //var methodCallConverter = new EntityTypeConverter();
            //var deserializerBuilder = new DeserializerBuilder()
            //    .WithNamingConvention(CamelCaseNamingConvention.Instance)
            //    .WithTypeConverter(methodCallConverter);

            //methodCallConverter.ValueDeserializer = deserializerBuilder.BuildValueDeserializer();

            //var deserializer = deserializerBuilder.Build();
            //var call = deserializer.Deserialize<EntityType>(yaml);
            //return call;
        }
    }
}
