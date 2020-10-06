using System.Collections.Generic;

namespace Ocelot.ConfigBuilder.Models
{
  public class OcelotConfig
  {
    public OcelotConfig()
    {
      GlobalConfiguration = new OcelotGlobalConfiguration();
    }

    public List<OcelotConfigRoute> Routes { get; set; } = new List<OcelotConfigRoute>();

    public OcelotGlobalConfiguration GlobalConfiguration { get; set; }
  }
}
