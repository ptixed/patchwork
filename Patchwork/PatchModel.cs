using Newtonsoft.Json.Linq;

namespace Patchwork
{
    public class PatchModel
    {
        public string Path { get; set; }
        public JObject Match { get; set; }
        public JObject Patch { get; set; }
    }
}
