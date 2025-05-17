using EveSdeModel.Interfaces;
using EveSdeModel.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace EveSdeModel.Factories
{
    public static class EveYamlFactory
    {
        public static T GetObject<T>(KeyValuePair<YamlNode, YamlNode> yamlNode) where T : IYamlEntity, new()
        {
            T result = new T();
            result.ParseWithId(yamlNode);
            return result;
        }

        public static T GetObject<T>(YamlMappingNode yamlNode) where T : IYamlEntity, new()
        {
            T result = new T();
            result.ParseNoId(yamlNode);
            return result;
        }

        public static void ParseNoId(IYamlEntity entity, YamlMappingNode yamlNode)
        {
            var type = entity.GetType();
            var properties = GetProperties(type);
            foreach (var node in yamlNode.Children)
            {
                var found = properties.FirstOrDefault(x => x.Name.GetAttr(type) == node.Key.ToString());
                if (found != null)
                {
                    found.SetValue(entity, node.Value.ToString());
                }
            }
        }

        private static Dictionary<Type, List<PropertyInfo>> Properties { get; } = [];
        public static List<PropertyInfo> GetProperties(Type type)
        {
            if (!Properties.ContainsKey(type))
            {
                Properties.Add(type, type.GetProperties().Where(x => x.PropertyType.Name == nameof(String)).ToList());
            }
            return Properties[type];
        }

        public static List<T> ParseFile<T>(string pathToFile) where T : IYamlEntity, new()
        {
            var result = new List<T>();
            using (var rd = new StreamReader(pathToFile))
            {
                // Load the stream
                var yaml = new YamlStream();
                yaml.Load(rd);

                // Examine the stream
                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
                foreach (var entry in mapping.Children)
                {
                    var _res = GetObject<T>(entry);
                    result.Add(_res);
                }
            }
            return result;
        }

        public static List<T> ParseFileSequence<T>(string pathToFile) where T : IYamlEntity, new()
        {
            var result = new List<T>();
            using (var rd = new StreamReader(pathToFile))
            {
                // Load the stream
                var yaml = new YamlStream();
                yaml.Load(rd);

                // Examine the stream
                var mapping = (YamlSequenceNode)yaml.Documents[0].RootNode;
                foreach (YamlMappingNode entry in mapping.Children)
                {
                    var _res = GetObject<T>(entry);
                    result.Add(_res);
                }
            }
            return result;
        }

        public static string GetAttr(this string attrName, Type type)
        {
            var result = type.GetProperty(attrName)?.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
            if (string.IsNullOrEmpty(result))
                result = attrName;
            return result;
        }

        public static string GetAttr<T>(this string attrName) where T : IYamlEntity, new()
        {
            return attrName.GetAttr(typeof(T));
        }
    }
}
