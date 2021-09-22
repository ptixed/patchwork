using System.Collections.Generic;

namespace Patchwork
{
    class PatchworkModel
    {
        public const string PatchworkKind = "Patchwork";

        public string Kind { get; set; }
        public List<string> Includes { get; set; }
        public List<PatchModel> Patches { get; set; }
        public Dictionary<string, string> Images { get; set; }
    }
}
