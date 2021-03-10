using Newtonsoft.Json.Linq;

namespace Patchwork
{
    public class PatchModel
    {
        public string PatchPath { get; set; }
        public JToken Match { get; set; }
        public JToken Patch { get; set; }
    }
}
