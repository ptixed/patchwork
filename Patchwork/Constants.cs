using System.Text.RegularExpressions;

namespace Patchwork
{
    static class Constants
    {
        public static readonly Regex YamlBoolRegex = new Regex("^(true|y|yes|on|false|n|no|off)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}
