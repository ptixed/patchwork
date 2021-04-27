using Newtonsoft.Json.Linq;

namespace Patchwork
{
    class KubernetesObject
    {
        public readonly JToken Object;
        public readonly string FilePath;
        public readonly int Priority;

        public KubernetesObject(JToken obj, string path)
        {
            Object = obj;
            FilePath = path;
            Priority = GetPriority(obj["kind"].ToString());
        }

        private static int GetPriority(string kind)
        {
            switch (kind)
            {
                case "Namespace":
                case "StorageClass":
                case "ClusterRole":
                    return 0;
                case "ConfigMap":
                case "Secret":
                case "PersistentVolumeClaim":
                case "ServiceAccount":
                case "Role":
                    return 1;
                case "Service":
                case "ClusterRoleBinding":
                case "RoleBinding":
                    return 2;
                case "CronJob":
                case "StatefulSet":
                case "Pod":
                case "Deployment":
                    return 3;
                case "Ingress":
                default:
                    return 4;
            }
        }
    }
}
