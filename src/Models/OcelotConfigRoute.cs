using System.Collections.Generic;

namespace Ocelot.ConfigBuilder.Models
{
  public class OcelotConfigRoute
  {
    public OcelotConfigRoute()
    {
      AddHeadersToRequest = new Dictionary<string, string>();
    }

    public string DownstreamPathTemplate { get; set; }
    public string DownstreamScheme { get; set; } = "http";
    public List<OcelotConfigBuilderDownstreamHost> DownstreamHostAndPorts { get; set; } = new List<OcelotConfigBuilderDownstreamHost>();

    public string UpstreamPathTemplate { get; set; }
    public string[] UpstreamHttpMethod { get; set; }

    public Dictionary<string, string> AddHeadersToRequest { get; set; }

    public OcelotConfigRouteAuthOptions AuthenticationOptions { get; set; }
  }
}
