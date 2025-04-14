using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.Utilities;
using YamlDotNet.Serialization;

using EveSdeModel.Models;
using EveSdeModel.Factories;

namespace EveSdeModel.Serialization
{
    public sealed class TypeMaterialConverter : IYamlTypeConverter
    {
        public IValueSerializer ValueSerializer { get; set; }
        public IValueDeserializer ValueDeserializer { get; set; }

        public bool Accepts(Type type) => type == typeof(TypeMaterial);

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            throw new NotImplementedException();
            //parser.Consume<MappingStart>();

            //var call = new TypeMaterial
            //{
            //    Id = (string)ValueDeserializer.DeserializeValue(parser, typeof(string), new SerializerState(), ValueDeserializer),
            //    Materials = (List<Material>)ValueDeserializer.DeserializeValue(parser, typeof(List<Material>), new SerializerState(), ValueDeserializer),
            //};

            //parser.Consume<MappingEnd>();

            //return call;
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            emitter.Emit(new MappingStart());

            var call = (TypeMaterial)value;
            ValueSerializer.SerializeValue(emitter, call.Id, typeof(string));
            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar(nameof(call.Materials).GetAttr<TypeMaterial>()));
            ValueSerializer.SerializeValue(emitter, call.Materials, typeof(List<Material>));
            emitter.Emit(new MappingEnd());

            emitter.Emit(new MappingEnd());
        }

        public static string SerializeTypeMaterial(TypeMaterial call)
        {
            var methodCallConverter = new TypeMaterialConverter();
            var serializerBuilder = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(methodCallConverter);

            methodCallConverter.ValueSerializer = serializerBuilder.BuildValueSerializer();

            var serializer = serializerBuilder.Build();

            var yaml = serializer.Serialize(call);
            return yaml;
        }

        public static TypeMaterial DeserializeTypeMaterial(string yaml)
        {
            throw new NotImplementedException();
            //var methodCallConverter = new TypeMaterialConverter();
            //var deserializerBuilder = new DeserializerBuilder()
            //    .WithNamingConvention(CamelCaseNamingConvention.Instance)
            //    .WithTypeConverter(methodCallConverter);

            //methodCallConverter.ValueDeserializer = deserializerBuilder.BuildValueDeserializer();

            //var deserializer = deserializerBuilder.Build();
            //var call = deserializer.Deserialize<TypeMaterial>(yaml);
            //return call;
        }
    }
}
