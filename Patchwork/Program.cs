using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Patchwork
{
    static class Program
    {
        private static readonly IDeserializer _deserializer = new DeserializerBuilder().Build();
        private static readonly ISerializer _serializer = new SerializerBuilder().JsonCompatible().Build();

        private static readonly JsonMergeSettings _merger = new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Merge
        };

        public static bool Matches(this JToken l, JToken r)
        {
            if (l?.GetType() != r?.GetType())
                return false;
            switch (l)
            {
                case JObject jo:
                    foreach (var prop in r as JObject)
                        if (!jo[prop.Key].Matches(prop.Value))
                            return false;
                    return true;
                case JValue jvl: 
                    var jvr = r as JValue;
                    if (jvl.Type == JTokenType.String && jvr.Type == JTokenType.String)
                        return Regex.IsMatch(jvl.Value.ToString(), $"^{jvr.Value}$");
                    return Equals(jvl.Value, jvr.Value);
            }
            throw new NotSupportedException();
        }

        private static IEnumerable<JToken> Process(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var reader = new StreamReader(stream))
            {
                var parser = new Parser(reader);
                parser.Consume<StreamStart>();
                while (parser.Accept<DocumentStart>(out var _))
                {
                    var yaml = _deserializer.Deserialize(parser);
                    var json = _serializer.Serialize(yaml);
                    var root = JsonConvert.DeserializeObject<JToken>(json);
                    
                    if (root["kind"].ToString() != PatchworkModel.PatchworkKind)
                    {
                        yield return root;
                        continue;
                    }

                    var patchwork = root.ToObject<PatchworkModel>();
                    var directory = Path.GetDirectoryName(path);
                    var filepaths = patchwork.Includes.Select(x => Path.Combine(directory, x)).ToList();
                    foreach (var file in filepaths.SelectMany(Process))
                    {
                        foreach (var patch in patchwork.Patches)
                            foreach (JObject branch in file.SelectTokens(patch.Path ?? ".").Where(x => x.Matches(patch.Match)))
                                branch.Merge(patch.Patch, _merger);
                        yield return file;
                    }
                }
            }
        }

        private static void Main(string[] args)
        {
            // args = new[] { "../../../test/provider/root.yml" };

            var files = Process(args[0]).ToArray();

            var serializer = new SerializerBuilder().Build();
            for (var i = 0; i < files.Length; ++i)
            {
                var json = JsonConvert.SerializeObject(files[i]);
                var root = _deserializer.Deserialize<object>(json);

                Console.Write(serializer.Serialize(root));
                Console.Write('\n');

                if (i < files.Length - 1)
                    Console.WriteLine("---\n");
            }
        }
    }
}
