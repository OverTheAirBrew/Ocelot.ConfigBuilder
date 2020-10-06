using System.Collections.Generic;
namespace Ocelot.ConfigBuilder.Kubernetes
{
    public class Configmap
    {
        public string apiVersion = "v1";
        public string kind = "ConfigMap";
        public ConfigmapMetadata Metadata { get; set; }
        public Dictionary<string, dynamic> Data = new Dictionary<string, dynamic>();
    }

    public class ConfigmapMetadata
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
    }
}
