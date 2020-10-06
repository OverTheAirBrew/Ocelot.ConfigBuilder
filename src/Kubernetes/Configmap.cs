using System.Collections.Generic;
namespace Ocelot.ConfigBuilder.Kubernetes
{
  public class Configmap
  {
    public string apiVersion = "v1";
    public string kind = "ConfigMap";
    public ConfigmapMetadata Metadata { get; set; }
    public Dictionary<string, string> Data = new Dictionary<string, string>();
  }

  public class ConfigmapMetadata
  {
    public string Name { get; set; }
    public string Namespace { get; set; }
  }
}
