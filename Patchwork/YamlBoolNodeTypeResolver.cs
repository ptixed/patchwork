using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Patchwork
{
    public class YamlBoolNodeTypeResolver : INodeTypeResolver
    {
        public bool Resolve(NodeEvent evt, ref Type type)
        {
            if (evt is Scalar s && s.Style == ScalarStyle.Plain)
            {
                if (Constants.YamlBoolRegex.IsMatch(s.Value))
                {
                    type = typeof(bool);
                    return true;
                }
                else if (double.TryParse(s.Value, out double _))
                {
                    type = typeof(double);
                    return true;
                }
            }
            return false;
        }
    }
}
