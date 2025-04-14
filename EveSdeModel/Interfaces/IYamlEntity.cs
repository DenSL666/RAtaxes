using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace EveSdeModel.Interfaces
{
    public interface IYamlEntity
    {
        void ParseWithId(KeyValuePair<YamlNode, YamlNode> yamlNode);
        void ParseNoId(YamlMappingNode yamlNode);
    }
}
