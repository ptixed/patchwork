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
        private static readonly IDeserializer _deserializer = new DeserializerBuilder()
            .WithNodeTypeResolver(new YamlBoolNodeTypeResolver())
            .Build();

        private static readonly ISerializer _serializer = new SerializerBuilder()
            .WithEventEmitter(next => new YamlBoolEventEmitter(next))
            .JsonCompatible()
            .Build();

        private static readonly JsonMergeSettings _merger = new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Concat
        };

        private static string FixPath(string path) => Path.GetRelativePath(Environment.CurrentDirectory, Path.GetFullPath(path));

        public static bool Matches(this JToken l, JToken r)
        {
            if (l?.GetType() != r?.GetType())
                return false;
            switch (l)
            {
                case JObject jol:
                    foreach (var prop in r as JObject)
                        if (!jol.TryGetValue(prop.Key, out var token) || !token.Matches(prop.Value))
                            return false;
                    return true;
                case JArray jal:
                    foreach (var item in r as JArray)
                        if (!jal.Any(x => x.Matches(item)))
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

        private static IEnumerable<(string objpath, JToken obj)> Process(string path)
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
                    
                    if (root[nameof(PatchworkModel.Kind).ToLower()]?.ToString() != PatchworkModel.PatchworkKind)
                    {
                        yield return (FixPath(path), root);
                        continue;
                    }

                    var patchwork = root.ToObject<PatchworkModel>();
                    var directory = Path.GetDirectoryName(path);
                    var filepaths = patchwork.Includes.Select(x => Path.Combine(directory, x)).ToList();
                    foreach (var (objpath, obj) in filepaths.SelectMany(Process))
                    {
                        foreach (var patch in patchwork.Patches)
                            if (obj.Matches(patch.Match))
                                foreach (JContainer node in obj.SelectTokens(patch.PatchPath ?? "$", true))
                                    node.Merge(patch.Patch, _merger);
                        yield return (objpath, obj);
                    }
                }
            }
        }

        private static int Main(string[] args)
        {
            //args = new[] { "../../../tests/sublevel/patchwork.yml", "../../../tests/ingress.yml" };

            if (args.Length == 0)
            {
                Console.WriteLine("usage: ./patchwork.exe path/to/patchwork/file.yml [path to render]...");
                return 1;
            }

            var objects = Process(args[0]).ToArray();
            if (args.Length > 1)
            {
                var paths = args.Skip(1).Select(FixPath).ToArray();
                objects = objects.Where(x => paths.Contains(x.objpath)).ToArray();
            }

            var serializer = new SerializerBuilder()
                .WithEventEmitter(next => new YamlBoolEventEmitter(next))
                .Build();

            for (var i = 0; i < objects.Length; ++i)
            {
                var json = JsonConvert.SerializeObject(objects[i].obj);
                var root = _deserializer.Deserialize<object>(json);

                Console.WriteLine($"# {objects[i].objpath}");
                Console.Write(serializer.Serialize(root));
                Console.WriteLine();

                if (i < objects.Length - 1)
                    Console.WriteLine("---");
            }

            return 0;
        }
    }
}
